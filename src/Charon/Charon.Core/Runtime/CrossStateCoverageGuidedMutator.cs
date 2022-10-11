using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Charon.Core.Dom;
using Charon.Core.IO;
using NLog;

namespace Charon.Core.Runtime
{
    public class CrossStateCoverageGuidedMutator
    {
		private static Charon.Core.Random ran = null;
		private static uint _lastIteration = 0;
		private static uint _lastSubIteration = 0;

        public static bool CoverageGuidedMutation(DataElement obj)
        {
            BitStream coverageGuidedMutatedValue = GetMutatedValue(obj);
            if(coverageGuidedMutatedValue != null){

				//DataModel的替换直接执行_value级别的替换，其他值不需要去更改，这样当DataModel Value.get()执行的时候，会直接拿这个值来使用
				//代价就是这里的_value要设置成Public的了，因为Value没有Set()函数
				obj._value = coverageGuidedMutatedValue;

				return true;
            }
			return false;
        }

        protected static BitStream GetMutatedValue(DataElement obj){
		
			if((!Charon.Core.Runtime.SHARE.if_in)) {
				
				Queue<DataModel> dataModelsToMutate = null;
				if(Charon.Core.Runtime.SHARE.queueLengthBeforeIteration != 0)
					dataModelsToMutate = Charon.Core.Runtime.SHARE.dataModelsToMutate;
				if(Charon.Core.Runtime.SHARE.seed_pool_to_use_cnt != 0)
					dataModelsToMutate = Charon.Core.Runtime.SHARE.valuableDataModels;
				
				if(dataModelsToMutate == null) {
					
					// Console.WriteLine("charon:Queue is empty,use own stratage!");

				} else {

					//添加概率
					if(ran == null || Charon.Core.Runtime.SHARE.CurIteration != _lastIteration || Charon.Core.Runtime.SHARE.CurSubIteration != _lastSubIteration)
					{
						_lastIteration = Charon.Core.Runtime.SHARE.CurIteration;
						_lastSubIteration = Charon.Core.Runtime.SHARE.CurSubIteration;
						ran = new Charon.Core.Random(_lastIteration * 7 + _lastSubIteration);
					}

					//选取队列中的第一个进行覆盖率引导
					DataModel _dataModelToMutate = dataModelsToMutate.Peek();
                    /*
                        执行跨状态覆盖率引导的条件：
                        1. 使用覆盖率引导算法
                        2. 当前DataElement是不可变异的
                        3. 当前DataElement是一个DataModel
                    */
					if((Charon.Core.Runtime.SHARE.ifuse) && (obj.isMutable == false) && (obj is DataModel)) {


						Charon.Core.Runtime.SHARE.should_relation_learn = false;
						Charon.Core.Runtime.SHARE.previous_datamodel =null;

						BitStream _sss = null;

						//_dataModelToMutate.isMutable == true 种子池里面的要是mutatable的DataModel，这样的DataModel对于替换来说是有意义的
						if(_dataModelToMutate.isMutable == true) {

							if(ran.Next(100) < 40) //概率 有40%的概率替换
							{
								string fullname1 = _dataModelToMutate.fullName.Substring(_dataModelToMutate.fullName.IndexOf(':'),_dataModelToMutate.fullName.Length-_dataModelToMutate.fullName.IndexOf(':'));
								string fullname2 = obj.fullName.Substring(obj.fullName.IndexOf(':'),obj.fullName.Length-obj.fullName.IndexOf(':'));
								//判断两个DataModel是否为一个
								if(fullname1 == fullname2) {
									
									//RTPSFuzzer策略正在执行，记录下当前起作用的previous_datamodel
									Charon.Core.Runtime.SHARE.previous_datamodel = _dataModelToMutate;

									// Console.WriteLine("charon:replace between states: set Value {1} from {0}", _dataModelToMutate.fullName, obj.fullName);
									return _dataModelToMutate._value;
								}
							}
						}
					} //end if
				}//end if			
			}//end if
            //没有执行覆盖率替换 return null
			return null;
		}
    }
}

// end
