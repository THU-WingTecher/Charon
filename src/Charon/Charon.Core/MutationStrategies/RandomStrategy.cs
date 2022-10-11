
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

using Charon.Core.IO;
using Charon.Core.Dom;
using Charon.Core.Cracker;

using Charon.Core.Runtime;

using NLog;

/*
 * If not 1st iteration, pick fandom data model to change
 * 
 */
namespace Charon.Core.MutationStrategies
{
	[DefaultMutationStrategy]
	[MutationStrategy("Random", true)]
	[MutationStrategy("RandomStrategy")]
	[Parameter("SwitchCount", typeof(int), "Number of iterations to perform per-mutator befor switching.", "200")]
	[Parameter("MaxFieldsToMutate", typeof(int), "Maximum fields to mutate at once.", "6")]
	public class RandomStrategy : MutationStrategy
	{
		class DataSetTracker
		{
			public List<Data> options = new List<Data>();
			public uint iteration = 1;
		};

		[Serializable]
		protected class ElementId : Tuple<string, string>
		{
			public ElementId(string modelName, string elementName)
				: base(modelName, elementName)
			{
			}

			public string ModelName { get { return Item1; } }
			public string ElementName { get { return Item2; } }
		}

		protected class Iterations : OrderedDictionary<ElementId, List<Mutator>> { }

		/*
			用于覆盖率反馈引导的对象集合_coverageGuidedElements 
			数据结构： 
				Key： DataElementContainer 一般是一个DataModel 
				Value：List<DataElement> 叶子节点组成的List
		*/
		protected class CoverageGuidedElements : Dictionary<ElementId,List<ElementId>> { }
		CoverageGuidedElements _coverageGuidedElements;

		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		OrderedDictionary<string, DataSetTracker> _dataSets;
		List<Type> _mutators;
		Iterations _iterations;
		KeyValuePair<ElementId, List<Mutator>>[] _mutations;

		/// <summary>
		/// container also contains states if we have mutations
		/// we can apply to them.  State names are prefixed with "STATE_" to avoid
		/// conflicting with data model names.
		/// Use a list to maintain the order this strategy learns about data models
		/// </summary>
		uint _iteration;
		Random _randomDataSet;
		uint _lastIteration = 1;

		/// <summary>
		/// How often to switch files.
		/// </summary>
		int switchCount = 200;

		/// <summary>
		/// Maximum number of fields to mutate at once.
		/// </summary>
		int maxFieldsToMutate = 6;

		public RandomStrategy(Dictionary<string, Variant> args)
			: base(args)
		{
			if (args.ContainsKey("SwitchCount"))
				switchCount = int.Parse((string)args["SwitchCount"]);
			if (args.ContainsKey("MaxFieldsToMutate"))
				maxFieldsToMutate = int.Parse((string)args["MaxFieldsToMutate"]);
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);

			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);
			Core.Dom.State.Starting += new StateStartingEventHandler(State_Starting);
			engine.IterationStarting += new Engine.IterationStartingEventHandler(engine_IterationStarting);
			_mutators = new List<Type>();
			_mutators.AddRange(EnumerateValidMutators());

			//注册一个Action Finish事件
			Core.Dom.Action.Finished += new ActionFinishedEventHandler(Action_Finishing);
		}

		void engine_IterationStarting(RunContext context, uint currentIteration, uint currentSubIteration, uint? totalIterations)
		{
			if (context.controlIteration && context.controlRecordingIteration)
			{
				_iterations = new Iterations();
				_dataSets = new OrderedDictionary<string, DataSetTracker>();
				_mutations = null;
			}
			else
			{
				// Random.Next() Doesn't include max and we want it to
				var fieldsToMutate = Random.Next(1, maxFieldsToMutate + 1);

				// if(SHARE.if_use_dyn==false){
				// 	_mutations = Random.Sample(_iterations, fieldsToMutate);
				// 	return;
				// }
				

				
				List<KeyValuePair<ElementId,List<Mutator>>> ret = new List<KeyValuePair<ElementId,List<Mutator>>>();
				// _iterations.ToList();
				List<int> usedIndexes = new List<int>();
				int index;
				if (_iterations.Count() < fieldsToMutate)
				{
					fieldsToMutate = _iterations.Count();
					ret.AddRange(_iterations);
					_mutations = Random.Shuffle(ret.ToArray());
				}else{
										//构造element_mutation_weight
					SHARE.element_mutation_weight.Clear();



					for(int i=0;i<_iterations.Count;i++){
						int points=1;
						var _iterations_item_key_at_i = _iterations.ElementAt(i).Key;
						if(false == SHARE.element_interesting.ContainsKey(_iterations_item_key_at_i)){
							points += 50;
						}else{
							var _element_interesting_item = SHARE.element_interesting[_iterations_item_key_at_i];
							points += (int)(_element_interesting_item[1]*5);
							points += (int)(_element_interesting_item[2]*200);
							int dispoint = (int)(20 - (this.Iteration - (int)_element_interesting_item[3]));
							if(dispoint > 0 && dispoint <=20){
								points += dispoint;
							}
						}
						SHARE.element_mutation_weight.Add(_iterations_item_key_at_i,points);
						
						if(SHARE.if_use_dyn == false){
							SHARE.element_mutation_weight[_iterations_item_key_at_i] = 1;
						}
					}

					//计算所有权重和
					int []sum = new int[_iterations.Count];
					int nowsum=0;
					for(int i=0;i<_iterations.Count;i++){
						nowsum+= SHARE.element_mutation_weight[_iterations.ElementAt(i).Key];
						sum[i] = nowsum;
					}
					Console.WriteLine("charon:element_mutation_weight sum : {0}",nowsum);
					for (int i = 0; i < fieldsToMutate; ++i)
					{
						do
						{
							index = Random.Next(0,nowsum);
							bool flag = false;
							for(int j=0;j<_iterations.Count-1;j++){
								if(sum[j]<=index&&sum[j+1]>index){
									index=j;
									flag = true;
									break;
								}	
							}
							if(flag == false){
								index = _iterations.Count-1;
							}
						}
						while (usedIndexes.Contains(index));
						usedIndexes.Add(index);
						ret.Add(_iterations.ElementAt(index));
					}




					// for (int i = 0; i < fieldsToMutate; ++i)
					// {
					// 	do
					// 	{
					// 		index = Random.Next(0, _iterations.Count());
					// 	}
					// 	while (usedIndexes.Contains(index));
					// 	usedIndexes.Add(index);

					// 	ret.Add(_iterations.ElementAt(index));
					// }



					_mutations = ret.ToArray();	
				}
				

				

				Console.WriteLine("charon:_iterations length: {0}, _mutations length: {1}",_iterations.Count,_mutations.Length);
				

			}
			
						Int64 unixTimestamp = (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
			Console.WriteLine("charon:iteration spend time: {0}",unixTimestamp - SHARE.time_iteration);
			SHARE.time_iteration = unixTimestamp;
		}

		public override void Finalize(RunContext context, Engine engine)
		{
			base.Finalize(context, engine);

			Core.Dom.Action.Starting -= Action_Starting;
			Core.Dom.State.Starting -= State_Starting;
			engine.IterationStarting -= engine_IterationStarting;
		}

		private uint GetSwitchIteration()
		{
			// Returns the iteration we should switch our dataSet based off our
			// current iteration. For example, if switchCount is 10, this function
			// will return 1, 11, 21, 31, 41, 51, etc.
			uint ret = _iteration - ((_iteration - 1) % (uint)switchCount);
			return ret;
		}

		public override bool IsDeterministic
		{
			get
			{
				return false;
			}
		}

		public override uint Iteration
		{
			get
			{
				return _iteration;
			}
			set
			{
				_iteration = value;
				SeedRandom();

				if (_iteration == GetSwitchIteration() && _lastIteration != _iteration)
					_randomDataSet = null;

				if (_randomDataSet == null)
				{
					logger.Debug("Iteration: Switch iteration, setting controlIteration and controlRecordingIteration.");

					_randomDataSet = new Random(this.Seed + GetSwitchIteration());

					_context.controlIteration = true;
					_context.controlRecordingIteration = true;
					_lastIteration = _iteration;
				}

				_mutations = null;
			}
		}

		void Action_Starting(Core.Dom.Action action)
		{
			//效果统计只能针对于Output的DataModel
			if(action.type == ActionType.Output){
				//判断ifCrossStateStrategyServes状态
				//实际上，A,B,C三个状态，A如果执行了crossState，那么可能对C也有影响
				//如果能传递，那么传递的条件是
				//	1. 上一轮的ifCrossStateStrategyServes已经为true并且上一轮仍在执行crossStateStrategy（即为mutable==false），不管成功不成功都要置为true
				//	2. 上一轮crossStateStrategy 已经为 true， 那么肯定要置为true
				//置为false的条件为上一轮 ifCrossStateStrategyServes 为false
				// if((SHARE.ifCrossStateStrategyServes == true && action.dataModel.isMutable == false ) || SHARE.ifCrossStateStrategyWorks == true){
				if(SHARE.ifCrossStateStrategyWorks == true || (SHARE.ifCrossStateStrategyTryToWork == true && SHARE.ifCrossStateStrategyServes == true)){
					SHARE.ifCrossStateStrategyServes = true;
				}else{
					SHARE.ifCrossStateStrategyServes = false;
				}

				//在ACtion开始时重置inState/crossState strategy works标志
				SHARE.ifInStateStrategyWorks = false;
				SHARE.ifCrossStateStrategyWorks = false;
				SHARE.ifCrossStateStrategyTryToWork = false;
			}

			// Is this a supported action?
			if (!(action.type == ActionType.Output || action.type == ActionType.SetProperty || action.type == ActionType.Call))
				return;

			if (_context.controlIteration && _context.controlRecordingIteration)
			{
				RecordDataSet(action);
				SyncDataSet(action);
				RecordDataModel(action);
			}
			else if (!_context.controlIteration)
			{
				MutateDataModel(action);
			}
		}

		void Action_Finishing(Core.Dom.Action action)
		{
			//效果统计只能针对于Output的DataModel
			if(action.type != ActionType.Output){
				return;
			}

			//统计总Action次数和总iteration数
			SHARE.totalAction++;

			//当前的action执行了什么策略
			// 0: Charon自身变异
			// 1: 有inStateCoverageGuided参与
			// 2: CrossStateCoverageGuided (这个一定是上个action)
			
			//当前的Action有没有触发新branch或新path
			if(true == Charon.Core.Runtime.SHARE.has_new_path){
				SHARE.totalInterestingAction++;
				if(SHARE.ifInStateStrategyWorks == true){
					SHARE.totalInStateInterestingAction++;
				}
				if(SHARE.ifCrossStateStrategyServes == true){
					SHARE.totalCrossStateInterestingAction++;
				}
			}

			Console.WriteLine(@"charon:reconsitution: CrossStateGuidance:  totalIteration:{0} {1} ; totalInterestingAction:{3} / {2} ;  totalInStateInterestingAction: {4} / {6} ; totalCrossStateInterestingAction: {5} / {7}",
			 DateTime.UtcNow.Subtract(SHARE.programStartTime), 
			 _iteration, SHARE.totalAction, 
			 SHARE.totalInterestingAction, 
			 SHARE.totalInStateInterestingAction, 
			 SHARE.totalCrossStateInterestingAction,
			 SHARE.totalInStateAction,
			 SHARE.totalCrossStateAction
			 );
		}

		void State_Starting(State state)
		{
			if (!_context.controlIteration || !_context.controlRecordingIteration)
				return;

			var key = new ElementId("STATE_" + state.name, null);

			if (_iterations.ContainsKey(key))
				return;

			List<Mutator> mutators = new List<Mutator>();

			foreach (Type t in _mutators)
			{
				// can add specific mutators here
				if (SupportedState(t, state))
				{
					var mutator = GetMutatorInstance(t, state);
					mutators.Add(mutator);
				}
			}

			if (mutators.Count > 0)
				_iterations.Add(key, mutators);
		}

		private DataModel ApplyFileData(Dom.Action action, Data data)
		{
			byte[] fileBytes = null;

			for (int i = 0; i < 5 && fileBytes == null; ++i)
			{
				try
				{
					fileBytes = File.ReadAllBytes(data.FileName);
				}
				catch (Exception ex)
				{
					logger.Debug("Failed to open '{0}'. {1}", data.FileName, ex.Message);
				}
			}

			if (fileBytes == null)
				throw new CrackingFailure(null, null);

			// Note: We need to find the origional data model to use.  Re-using
			// a data model that has been cracked into will fail in odd ways.
			var dataModel = GetNewDataModel(action);

			// Crack the file
			DataCracker cracker = new DataCracker();
			cracker.CrackData(dataModel, new BitStream(fileBytes));

			return dataModel;
		}

		private DataModel AppleFieldData(Dom.Action action, Data data)
		{
			// Note: We need to find the origional data model to use.  Re-using
			// a data model that has been cracked into will fail in odd ways.
			var dataModel = GetNewDataModel(action);

			// Apply the fields
			data.ApplyFields(dataModel);

			return dataModel;
		}

		private DataModel GetNewDataModel(Dom.Action action)
		{
			var referenceName = action.dataModel.referenceName;
			if (referenceName == null)
				referenceName = action.dataModel.name;

			var sm = action.parent.parent;
			Dom.Dom dom = _context.dom;

			int i = sm.name.IndexOf(':');
			if (i > -1)
			{
				string prefix = sm.name.Substring(0, i);

				Dom.Dom other;
				if (!_context.dom.ns.TryGetValue(prefix, out other))
					throw new CharonException("Unable to locate namespace '" + prefix + "' in state model '" + sm.name + "'.");

				dom = other;
			}

			// Need to take namespaces into account when searching for the model
			var baseModel = dom.getRef<DataModel>(referenceName, a => a.dataModels);

			var dataModel = baseModel.Clone() as DataModel;
			dataModel.isReference = true;
			dataModel.referenceName = referenceName;

			return dataModel;
		}

		private void SyncDataSet(Dom.Action action)
		{
			System.Diagnostics.Debug.Assert(_iteration != 0);

			// Only sync <Data> elements if the action has a data model
			if (action.dataModel == null)
				return;

			string key = GetDataModelName(action);
			DataSetTracker val = null;
			if (!_dataSets.TryGetValue(key, out val))
				return;

			// If the last switch was within the current iteration range then we don't have to switch.
			uint switchIteration = GetSwitchIteration();
			if (switchIteration == val.iteration)
				return;

			// Don't switch files if we are only using a single file :)
			if (val.options.Count < 2)
				return;

			DataModel dataModel = null;

			// Some of our sample files may not crack.  Loop through them until we
			// find a good sample file.
			while (val.options.Count > 0 && dataModel == null)
			{
				Data option = _randomDataSet.Choice(val.options);

				if (option.DataType == DataType.File)
				{
					try
					{
						dataModel = ApplyFileData(action, option);
					}
					catch (CrackingFailure)
					{
						logger.Debug("Removing " + option.FileName + " from sample list.  Unable to crack.");
						val.options.Remove(option);
					}
				}
				else if (option.DataType == DataType.Fields)
				{
					try
					{
						dataModel = AppleFieldData(action, option);
					}
					catch (CharonException)
					{
						logger.Debug("Removing " + option.name + " from sample list.  Unable to apply fields.");
						val.options.Remove(option);
					}
				}
			}

			if (dataModel == null)
				throw new CharonException("Error, RandomStrategy was unable to load data for model \"" + action.dataModel.fullName + "\"");

			// Set new data model
			action.dataModel = dataModel;

			// Generate all values;
			var ret = action.dataModel.Value;
			System.Diagnostics.Debug.Assert(ret != null);

			// Store copy of new origional data model
			action.origionalDataModel = action.dataModel.Clone() as DataModel;

			// Save our current state
			val.iteration = switchIteration;
		}

		private void GatherMutators(string modelName, DataElementContainer cont)
		{
			List<DataElement> allElements = new List<DataElement>();
			RecursevlyGetElements(cont, allElements);
			foreach (DataElement elem in allElements)
			{
				var elementName = elem.fullName;
				List<Mutator> mutators = new List<Mutator>();

				foreach (Type t in _mutators)
				{
					// can add specific mutators here
					if (SupportedDataElement(t, elem))
					{
						var mutator = GetMutatorInstance(t, elem);
						mutators.Add(mutator);
					}
				}

				if (mutators.Count > 0){
					_iterations.Add(new ElementId(modelName, elementName), mutators);
				}			
			}
			

			//构造用于覆盖率反馈引导的对象集合_coverageGuidedElements
			//清空_coverageGuidedElements中的每一项
			ElementId containerElementID = new ElementId(modelName, cont.fullName);
			if(_coverageGuidedElements == null){
				_coverageGuidedElements = new CoverageGuidedElements();
			}
			if(_coverageGuidedElements.ContainsKey(containerElementID)==false){
				_coverageGuidedElements.Add(containerElementID, new List<ElementId>());
			}else{
				_coverageGuidedElements[containerElementID].Clear();
			}
			//构建datamodel/action和可变异的叶子节点dataelements之间的对应关系 这些都是状态内覆盖率反馈进行变异的对象
			foreach (DataElement elem in allElements){
				if((!(elem is Block))&& (elem.isMutable == true)){
					_coverageGuidedElements[containerElementID].Add(new ElementId(modelName, elem.fullName));
				}
			}
		}

		private void RecordDataModel(Core.Dom.Action action)
		{
			if (action.dataModel != null)
			{
				string modelName = GetDataModelName(action);
				GatherMutators(modelName, action.dataModel);
			}
			else if (action.parameters != null)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string modelName = GetDataModelName(action, param);
						GatherMutators(modelName, param.dataModel);
					}
				}
			}
		}

		private void RecordDataSet(Core.Dom.Action action)
		{
			if (action.dataSet != null)
			{
				DataSetTracker val = new DataSetTracker();
				foreach (var item in action.dataSet.Datas)
				{
					switch (item.DataType)
					{
						case DataType.File:
							val.options.Add(item);
							break;
						case DataType.Files:
							val.options.AddRange(item.Files.Select(a => new Data() { DataType = DataType.File, FileName = a }));
							break;
						case DataType.Fields:
							val.options.Add(item);
							break;
						default:
							throw new CharonException("Unexpected DataType: " + item.DataType.ToString());
					}
				}

				if (val.options.Count > 0)
				{
					// Need to properly support more than one action that are unnamed
					string key = GetDataModelName(action);
					System.Diagnostics.Debug.Assert(!_dataSets.ContainsKey(key));
					_dataSets.Add(key, val);
				}
			}

		}

		private void ApplyMutation(string modelName, DataModel dataModel)
		{
						SHARE.mutation_element = new List<Tuple<string, string>>();
						dataModel.mutated_element_list.Clear();

			
			List<Tuple<string,string>> mutated = new List<Tuple<string, string>>();


						if(SHARE.if_use_dyn==true&&SHARE.if_use_relation_learning==true&&SHARE.previous_datamodel!=null&&dataModel.isMutable==true){
				//构造当前datamodel的list weight
				Dictionary<Tuple<string,string>,int> this_datamodel_mutation_weight = new Dictionary<Tuple<string, string>, int>();
				Dictionary<Tuple<string,string>,List<Mutator>> tempiterations = new Dictionary<Tuple<string, string>, List<Mutator>>();
				foreach(var item in _iterations){
					if (item.Key.ModelName != modelName)
						continue;
					var elem = dataModel.find(item.Key.ElementName);
					if(elem!=null){
						this_datamodel_mutation_weight.Add(item.Key,SHARE.element_mutation_weight[item.Key]);
						tempiterations.Add(item.Key,item.Value);
					}
				}

				System.Console.WriteLine("charon:mutation learning debug:prepare ends");

				foreach(var item1 in SHARE.previous_datamodel.mutated_element_list){
					if(SHARE.relation_table.ContainsKey(item1)==false){
						continue;
					}
					foreach(var item2 in SHARE.relation_table[item1]){
						if(this_datamodel_mutation_weight.ContainsKey(item2.Key)==false){
							continue;
						}
						//每一个relation算5分
						this_datamodel_mutation_weight[item2.Key] += item2.Value*15;
					}
				}

				System.Console.WriteLine("charon:mutation learning debug:cal ends");

				//计算所有权重和 并选取 变异
				int []sum = new int[this_datamodel_mutation_weight.Count];
				int nowsum=0;
				for(int i=0;i<this_datamodel_mutation_weight.Count;i++){
					nowsum+= this_datamodel_mutation_weight.ElementAt(i).Value;
					sum[i] = nowsum;
				}
				var fieldsToMutate = Random.Next(1, 6 + 1);
				List<int> usedIndexes = new List<int>();
				for (int i = 0; i < fieldsToMutate; ++i)
				{
					int index;
					do
					{
						index = Random.Next(0,nowsum);
						bool flag = false;
						for(int j=0;j<this_datamodel_mutation_weight.Count-1;j++){
							if(sum[j]<=index&&sum[j+1]>index){
								index=j;
								flag = true;
								break;
							}	
						}
						if(flag == false){
							index = this_datamodel_mutation_weight.Count-1;
						}
					}
					while (usedIndexes.Contains(index));
					usedIndexes.Add(index);
					var key = this_datamodel_mutation_weight.ElementAt(index).Key;


					if(mutated.Contains(key)){
						continue;
					}else{
						mutated.Add(key);
					}

					var elem = dataModel.find(key.Item2);
					if(elem==null){
						continue;
					}
					//开始变异
					Mutator mutator = Random.Choice(tempiterations[key]);
					OnMutating(key.Item2, mutator.name);
					logger.Debug("Action_Starting: Fuzzing: " + key.Item2);
					logger.Debug("Action_Starting: Mutator: " + mutator.name);
					mutator.randomMutation(elem);


										SHARE.mutation_element.Add(key);
										if(false == SHARE.element_interesting.ContainsKey(key)){
						SHARE.element_interesting.Add(key,new double[4]{1.0,0.0,0.0,0.0});
					}
					else{
						SHARE.element_interesting[key][0] += 1.0;
					}
				}
				System.Console.WriteLine("charon:mutation learning works!");
			}



			//Charon自身变异策略
			foreach (var item in _mutations)
			{
				if (item.Key.ModelName != modelName)
					continue;

				var elem = dataModel.find(item.Key.ElementName);
				if (elem != null)
				{

					if(mutated.Contains(item.Key)){
						continue;
					}else
					{
						mutated.Add(item.Key);
					}

										dataModel.mutated_element_list.Add(item.Key);

					Mutator mutator = Random.Choice(item.Value);
					OnMutating(item.Key.ElementName, mutator.name);
					logger.Debug("Action_Starting: Fuzzing: " + item.Key.ElementName);
					logger.Debug("Action_Starting: Mutator: " + mutator.name);
					mutator.randomMutation(elem);

					var newt = new Tuple<string, string>(modelName,item.Key.ElementName);


										SHARE.mutation_element.Add(newt);
										if(false == SHARE.element_interesting.ContainsKey(newt)){
						SHARE.element_interesting.Add(newt,new double[4]{1.0,0.0,0.0,0.0});
					}
					else{
						SHARE.element_interesting[newt][0] += 1.0;
					}
					//every time which element was mutated.
					// int length = _mutations.Length;
					// SHARE.mutation_element = new List<Tuple<string, string>>();
					// for(int i=0;i<length;i++){
					// 	ElementId this_id=_mutations[i].Key;
					// 	string model_name=this_id.ModelName;
					// 	string element_name=this_id.ElementName;
					// }
				}
				else
				{
					logger.Debug("Action_Starting: Skipping Fuzzing: " + item.Key.ElementName);
				}
			}
		


			//执行覆盖率引导的element变异（状态内部）


			//debug:输出所有_coverageGuidedElements
			// foreach(var _item in _coverageGuidedElements){
			// 	Console.Write("charon:Reconsitution: {0}",_item.Key.ElementName);
			// 	foreach(var _d in _item.Value){
			// 		Console.Write(" {0} ",_d.ElementName);
			// 	}
			// 	Console.Write("\n");
			// }

			ElementId containerElementID = new ElementId(modelName,dataModel.fullName);
			
			// Console.WriteLine("charon:Reconsitution: CoverageGuidedMutation attempts to work! Contains key? :{0} {1}",dataModel.fullName,_coverageGuidedElements.ContainsKey(containerElementID));
			if(_coverageGuidedElements.ContainsKey(containerElementID)){
				var _elements =  _coverageGuidedElements[containerElementID];
				foreach(var _element in _elements){
					//保证datamodel对应
					if(_element == null || _element.ModelName != modelName){
						continue;
					}
					var _thatElement=dataModel.find(_element.ElementName);
					// Console.WriteLine("charon:Reconsitution: CoverageGuidedMutation attempts to work! object:{0} {1}",_element.ElementName, _thatElement==null?"null":_thatElement.fullName);
					//去除被变异后已经消失的element和其他已经变异的element
					if(_thatElement==null||mutated.Contains(_element)){
						continue;
					}
					bool result = Core.Runtime.InStateCoverageGuidedMutator.CoverageGuidedMutation(_thatElement);
					//如果在这个action中，有一个element被成功执行了instate策略，那么这个action的ifInStateStrategyWorks标志就为true
					if(result == true){
						SHARE.ifInStateStrategyWorks = true;
					}
					// Console.WriteLine("charon:Reconsitution: CoverageGuidedMutation attempts to work! Result:{0}",result);
				}
			}
			if(SHARE.ifInStateStrategyWorks == true){
				SHARE.totalInStateAction++;
			}



			//执行覆盖率引导的DataModel变异 (跨状态)
			bool crossStateGuidanceResult = Core.Runtime.CrossStateCoverageGuidedMutator.CoverageGuidedMutation(dataModel);
			if(crossStateGuidanceResult == true){
				SHARE.ifCrossStateStrategyWorks = true;
				SHARE.totalCrossStateAction++;
				Console.WriteLine("charon:reconsitution: CrossStateGuidance: strategy works and the _value has been replaced. {0}", dataModel.fullName);
			}else{
				Console.WriteLine("charon:reconsitution: CrossStateGuidance: strategy attampted to work but failed. {0}", dataModel.fullName);
			}

			if((SHARE.ifuse) && (dataModel.isMutable == false)) {
				//CrossStateStrategy尝试在执行
				Charon.Core.Runtime.SHARE.ifCrossStateStrategyTryToWork = true;	
			}
		}

		private void MutateDataModel(Core.Dom.Action action)
		{
			// MutateDataModel should only be called after ParseDataModel
			System.Diagnostics.Debug.Assert(_iteration > 0);

			if (action.dataModel != null)
			{
				string modelName = GetDataModelName(action);
				ApplyMutation(modelName, action.dataModel);
			}
			else if (action.parameters != null)
			{
				foreach (var param in action.parameters)
				{
					if (param.dataModel != null)
					{
						string modelName = GetDataModelName(action, param);
						ApplyMutation(modelName, param.dataModel);
					}
				}
			}
		}

		/// <summary>
		/// Allows mutation strategy to affect state change.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		public override State MutateChangingState(State state)
		{
			if (_context.controlIteration)
				return state;

			string name = "STATE_" + state.name;

			foreach (var item in _mutations)
			{
				if (item.Key.ModelName != name)
					continue;

				Mutator mutator = Random.Choice(item.Value);
				OnMutating(state.name, mutator.name);

				logger.Debug("MutateChangingState: Fuzzing state change: " + state.name);
				logger.Debug("MutateChangingState: Mutator: " + mutator.name);

				return mutator.changeState(state);
			}

			return state;
		}

		public override uint Count
		{
			get
			{
				return uint.MaxValue;
			}
		}
	}
}

// end
