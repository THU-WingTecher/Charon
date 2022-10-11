using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Charon.Core.Dom;
using Charon.Core.IO;
using NLog;

namespace Charon.Core.Runtime
{
    public class InStateCoverageGuidedMutator
    {
		private static Charon.Core.Random ran = null;
		private static uint _lastIteration = 0;
		private static uint _lastSubIteration = 0;

        public static bool CoverageGuidedMutation(DataElement obj)
        {
            BitStream coverageGuidedMutatedValue = charonGetMutatedValue(obj);
            if(coverageGuidedMutatedValue != null){

				obj.MutatedValue = new Variant(coverageGuidedMutatedValue);
                //对所有基于覆盖率的变异 打上标签 default
				obj.mutationFlags = DataElement.MUTATE_DEFAULT;
				//对String类型的变异，变异可能破坏internal value 所以 要打上 type transform的标签，使stream直接生效
				if("String" == obj.elementType || "Number" == obj.elementType){
					obj.mutationFlags |= DataElement.MUTATE_OVERRIDE_TYPE_TRANSFORM;
					// Console.WriteLine("charon:Reconsitution: string: {0}",(string)obj.InternalValue);
					// Console.WriteLine("charon:Reconsitution: string: {0}",(string)coverageGuidedMutatedValue);
				}
				return true;
            }
			return false;
        }

        protected static BitStream charonGetMutatedValue(DataElement obj){
		
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
                        执行覆盖率引导的条件：
                        1. 使用覆盖率引导算法
                        2. 当前DataElement是可变异的
                        3. 当前DataElement是一个叶子节点
                    */
					if((Charon.Core.Runtime.SHARE.ifuse) && (!Charon.Core.Runtime.SHARE.if_ban_guided) && (obj.isMutable == true) && !(obj is Block)) {

						// Console.WriteLine("charon: replace among datamodels within one state");

						foreach(DataElement dataElement in  _dataModelToMutate.EnumerateAllElements() ){


                            //名称一致，认为是同类型的block，可进行替换 这里的name是最简name；
                            //同样这里判断叶子节点
							// if((dataElement.name == obj.name) && (!(dataElement is Block))){
							bool result1 = (dataElement.hasLength==true) && (obj.hasLength==true) &&(dataElement.lengthAsBits == obj.lengthAsBits);
							bool result2 =  (dataElement.name == obj.name);
							if( (dataElement.elementType == obj.elementType) && (result1 || result2) && (!(dataElement is Block))){
								// Console.WriteLine("charon:Reconsitution: {0} {1}",dataElement.elementType,obj.elementType);

                                //todo：需要判断是否是相同值，是相同值替换没有意义 使用equal方法判断，而不是“=”
                                if(BytesArrayEquals(dataElement.Value.Value,obj.Value.Value)){
									// Console.WriteLine("charon:Reconsitution: value equal, jump!");
                                    continue;
                                }

								// Console.WriteLine("charon:Reconsitution: value unequal! {0} {1}",dataElement.Value.Value,obj.Value.Value);

                                //todo: 重构 首先搜集所有不同的_value，然后随机选取？
                                // 1/3的概率跳过 不一定要取第一个符合的block
								if(ran.Next(120)%3 == 0) {
									continue;
								}

								Charon.Core.Runtime.SHARE.if_in = false;
								Charon.Core.Runtime.SHARE.if_replace_just_now = true;
								
                                //执行return 认为已成功执行覆盖率引导
								return dataElement.Value;
							}
						}//end foreach
					} //end if
				}//end if			
			}//end if
            //始终没有执行覆盖率替换 return null
			return null;
		}

		private static bool BytesArrayEquals(byte[] b1, byte[] b2)
    	{
			if (b1.Length != b2.Length) return false;
			if (b1 == null || b2 == null) return false;
			for (int i = 0; i < b1.Length; i++)
				if (b1[i] != b2[i])
					return false;
			return true;
    	}
    }
}

// end
