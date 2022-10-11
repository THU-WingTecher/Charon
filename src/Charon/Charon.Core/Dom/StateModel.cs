﻿
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
using System.Threading;
using System.Xml;

using Charon.Core;
using Charon.Core.IO;

using NLog;

using System.Runtime.InteropServices;

namespace Charon.Core.Dom
{
	public delegate void StateModelStartingEventHandler(StateModel model);
	public delegate void StateModelFinishedEventHandler(StateModel model);

	[Serializable]
	public class StateModel : INamed
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();
		static uint last_Iteration = 1;

		public string _name = null;
		public object parent;
		protected State _initialState = null;
		public List<Action> dataActions = new List<Action>();

		/// <summary>
		/// All states in state model.
		/// </summary>
		public Dictionary<string, State> states = new Dictionary<string, State>();

		public string name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// The initial state to run when state machine executes.
		/// </summary>
		public State initialState
		{
			get
			{
				return _initialState;
			}

			set
			{
				_initialState = value;
			}
		}

		/// <summary>
		/// StateModel is starting to execute.
		/// </summary>
		public static event StateModelStartingEventHandler Starting;
		/// <summary>
		/// StateModel has finished executing.
		/// </summary>
		public static event StateModelFinishedEventHandler Finished;

		protected virtual void OnStarting()
		{
			if (Starting != null)
				Starting(this);
		}

		protected virtual void OnFinished()
		{
			if (Finished != null)
				Finished(this);
		}

				[DllImport(@"charonControl", EntryPoint="charon_update")]
        public static unsafe extern int charon_update(int iteration);

		/// <summary>
		/// Start running the State Machine
		/// </summary>
		/// <remarks>
		/// This will start the initial State.
		/// </remarks>
		/// <param name="context"></param>
		public void Run(RunContext context)
		{
			try
			{
				OnStarting();

				foreach (Publisher publisher in context.test.publishers.Values)
				{
					publisher.Iteration = context.test.strategy.Iteration;
					publisher.IsControlIteration = context.controlIteration;
				}

				dataActions.Clear();

				// Prior to starting our state model, on iteration #1 lets
				// locate all data sets and load our initial data.
				//
				// Additionally this is were we setup origionalDataModel.
				//
				if (context.needDataModel)
				{
					context.needDataModel = false;

					foreach (State state in states.Values)
					{
						foreach (Action action in state.actions)
						{
							if (action.dataModel != null && action.dataSet != null && action.dataSet.Datas.Count > 0)
							{
								Data data = action.dataSet.Datas[0];
								string fileName = null;

								if (data.DataType == DataType.File)
								{
									fileName = data.FileName;

									try
									{
										logger.Debug("Trying to crack " + fileName);
										Cracker.DataCracker cracker = new Cracker.DataCracker();
										cracker.CrackData(action.dataModel,
											new BitStream(File.OpenRead(fileName)));
									}
									catch (Cracker.CrackingFailure ex)
									{
										throw new CharonException("Error, failed to crack \"" + fileName +
											"\" into \"" + action.dataModel.fullName + "\": " + ex.Message, ex);
									}
								}
								else if (data.DataType == DataType.Files)
								{
									bool success = false;
									foreach (var fn in data.Files)
									{
										try
										{
											logger.Debug("Trying to crack " + fn);
											fileName = fn;

											Cracker.DataCracker cracker = new Cracker.DataCracker();
											cracker.CrackData(action.dataModel,
												new BitStream(File.OpenRead(fileName)));

											success = true;
											break;
										}
										catch
										{
											logger.Debug("Cracking failed, trying next file");
										}
									}
									
									if(!success)
										throw new CharonException("Error, failed to crack any of the files specified by action \"" + action.name + "\".");
								}
								
								// Always apply fields if we have them
								if (data.fields.Count > 0)
								{
									data.ApplyFields(action.dataModel);
								}

								var value = action.dataModel.Value;
								System.Diagnostics.Debug.Assert(value != null);

								// Update our origional copy to have data!
								action.origionalDataModel = action.dataModel.Clone() as DataModel;
							}
							else if (action.dataModel != null)
							{
								var value = action.dataModel.Value;
								System.Diagnostics.Debug.Assert(value != null);

								// Update our origional copy to have data!
								action.origionalDataModel = action.dataModel.Clone() as DataModel;
							}
							else if (action.parameters.Count > 0)
							{
								foreach (ActionParameter param in action.parameters)
								{
									if (param.dataModel != null && param.data != null)
									{
										Data data = param.data as Data;
										string fileName = null;

										if (data.DataType == DataType.File)
											fileName = data.FileName;
										else if (data.DataType == DataType.Files)
											fileName = data.Files[0];
										else
											data.ApplyFields(param.dataModel);

										if (fileName != null)
										{
											try
											{
												Cracker.DataCracker cracker = new Cracker.DataCracker();
												cracker.CrackData(param.dataModel,
													new BitStream(File.OpenRead(fileName)));
											}
											catch (Cracker.CrackingFailure ex)
											{
												throw new CharonException("Error, failed to crack \"" + fileName + 
													"\" into \"" + action.dataModel.fullName + "\": " + ex.Message, ex);
											}
										}
									}

									// Invalidate model and produce value
									var value = param.dataModel.Value;
									System.Diagnostics.Debug.Assert(value != null);

									// Update our origional copy to have data!
									param.origionalDataModel = param.dataModel.Clone() as DataModel;
								}
							}
						}
					}
				}

				// Update all data model to clones of origionalDataModel
				// before we start down the state path.
				foreach (State state in states.Values)
				{
					state.runCount = 0;

					foreach (Action action in state.actions)
						action.UpdateToOrigionalDataModel();
				}

				State currentState = _initialState;

				while (true)
				{
					try
					{
						currentState.Run(context);
						break;
					}
					catch (ActionChangeStateException ase)
					{
						var newState = context.test.strategy.MutateChangingState(ase.changeToState);
						
						if(newState == ase.changeToState)
							logger.Debug("Run(): Changing to state \"" + newState.name + "\".");
						else
							logger.Debug("Run(): Changing state mutated.  Switching to \"" + newState.name + 
								"\" instead of \""+ase.changeToState+"\".");
						
						currentState.OnChanging(newState);
						currentState = newState;
					}
				}
			}
			catch (ActionException)
			{
				// Exit state model!
			}
			finally
			{
				//当前非repo的叠加模式 
				if(!(Charon.Core.Runtime.SHARE.if_CharonStarRepo && (context.test.strategy.Iteration < Charon.Core.Runtime.SHARE.CharonStarRepoStartIteration))){
					
					//iteration stop, dequeue;
					if(Charon.Core.Runtime.SHARE.queueLengthBeforeIteration != 0){
						Console.WriteLine("charon: Iteration finish! Dequeue!");

						
						if(Charon.Core.Runtime.SHARE.has_new_path_iteration)
						{
							DataModel _dataModel = Charon.Core.Runtime.SHARE.dataModelsToMutate.Peek();
							_dataModel.use_time = 0;
							Charon.Core.Runtime.SHARE.valuableDataModels.Enqueue(_dataModel.Clone() as DataModel);
							//更新Index列表
							Charon.Core.Runtime.SHARE.seedPoolIndexQueue.Enqueue(++Charon.Core.Runtime.SHARE.seedPoolIndex);
							//保存种子到本地
							Charon.Core.Runtime.SHARE.saveNewSeedToFile(_dataModel.Clone() as DataModel,(Charon.Core.Loggers.FileLogger)context.test.loggers[0]);
						}
								
						Charon.Core.Runtime.SHARE.dataModelsToMutate.Dequeue();
						Charon.Core.Runtime.SHARE.queueLengthBeforeIteration--;
					}
					else if (Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt != 0)
					{
						DataModel _dataModel = Charon.Core.Runtime.SHARE.valuableDataModels.Peek();
						//取出此时的index
						int seedIndex = Charon.Core.Runtime.SHARE.seedPoolIndexQueue.Peek();
						Charon.Core.Runtime.SHARE.seedPoolIndexQueue.Dequeue();
						Charon.Core.Runtime.SHARE.valuableDataModels.Dequeue();
						if(Charon.Core.Runtime.SHARE.has_new_path_iteration)
							_dataModel.use_time = 0;
						else
							_dataModel.use_time++;
						if(_dataModel.use_time < Charon.Core.Runtime.SHARE.use_time_limit)
						{
							Charon.Core.Runtime.SHARE.seedPoolIndexQueue.Enqueue(seedIndex);
							Charon.Core.Runtime.SHARE.valuableDataModels.Enqueue(_dataModel.Clone() as DataModel);
						}
						Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt--;
					}
					else{
						Console.WriteLine("charon: Iteration finish! No Dequeue!");
					}

										if(Charon.Core.Runtime.SHARE.queueLengthBeforeIteration == 0 && Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt == 0 && Charon.Core.Runtime.SHARE.dataModelsToMutate.Count==0){
						//todo：调用control.c
						Console.WriteLine("charon:charon_update start");
						unsafe{
							if(0 != charon_update((int)context.test.strategy.Iteration + 1)){
								Console.WriteLine("charon:charon_update run error!");
							}
						}
						//还要更新seedIndexQueueCopy,为seedIndexQueue的快照
						Charon.Core.Runtime.SHARE.seedPoolIndexQueueCopy = new Queue<int>(Charon.Core.Runtime.SHARE.seedPoolIndexQueue);
						Console.WriteLine("charon:charon_update finish");
					}
					
					//change queue length
					if((Charon.Core.Runtime.SHARE.ifuse) && last_Iteration != context.test.strategy.Iteration) //only execute when sub_iteration equals 0 
					{
						last_Iteration = context.test.strategy.Iteration;
						Charon.Core.Runtime.SHARE.queueLengthBeforeIteration = Charon.Core.Runtime.SHARE.dataModelsToMutate.Count;

						if(Charon.Core.Runtime.SHARE.queueLengthBeforeIteration == 0)
						{
							if(Charon.Core.Runtime.SHARE.valuableDataModels.Count != 0)
							{
								Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt = Math.Min(Charon.Core.Runtime.SHARE.valuableDataModels.Count, Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt_limit);
							}
							else 
							{
								Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt = 0;
							}

						}


						// add CharonWather
						string sPath = Charon.Core.Runtime.SHARE.pathWather;
						if (!File.Exists(sPath))  
						{ 
							FileStream fs = new FileStream(sPath, FileMode.Create, FileAccess.Write); 
							StreamWriter sw = new StreamWriter(fs); 
							StringBuilder sb = new StringBuilder(); 
							sb.Append("Iteration").Append(",").Append("From last iteration").Append(",").Append("From seed pool").Append(",").Append("Seed pool size"); 
							sw.WriteLine(sb); 
							sw.Flush();
							sw.Close();
							fs.Close();
						} 
						var csv = new StringBuilder(); 
						var newLine = string.Format("{0},{1},{2},{3}", Charon.Core.Runtime.SHARE.CurIteration , Charon.Core.Runtime.SHARE.queueLengthBeforeIteration,
																Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt, Charon.Core.Runtime.SHARE.valuableDataModels.Count);
						csv.AppendLine(newLine);   
						File.AppendAllText(sPath, csv.ToString()); 
						Console.WriteLine("Add {0} sub iterations...", Charon.Core.Runtime.SHARE.queueLengthBeforeIteration);
					}

					Charon.Core.Runtime.SHARE.has_new_path_iteration = false;
				}

				
				
				foreach (Publisher publisher in context.test.publishers.Values){
					try{
						publisher.close();
						Console.WriteLine("charon:opps! publisher close success!");
					}
					catch(Exception e){
						Console.WriteLine("charon:opps! publisher close failed!" + e.ToString());
					}
					
				}
					

				OnFinished();
				Console.WriteLine("charon:state finish success!");
			}
		}
	}
}

// END
