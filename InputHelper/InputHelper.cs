using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InputHelper
{
    public static class InputHelperManager
    {
        private static readonly InputHelperManagerDataModel DataModel= new InputHelperManagerDataModel();

        /// <summary>
        /// 使用前初始化
        /// </summary>
        /// <param name="isUseMap">是否启用事件映射</param>
        /// <param name="axesMap">事件映射的配置文件</param>
        public static void Initialization(bool isUseMap = false, AxesMap axesMap = null)
        {
            DataModel.NoReserveInputActions = new Stack<InputActionEntity>();
            DataModel.InputActions = new Stack<InputActionEntity>();
            DataModel.InputActionsPool = new Stack<InputActionEntity>();
            DataModel.AxesMap = new Dictionary<string, string>();
            DataModel.IsUseMap = isUseMap;
            if (isUseMap)
            {
                if (axesMap == null)
                {
                    Debug.LogError("启用映射时 axesMap 不能为空");
                }
                else
                {
                    foreach (var element in axesMap.entity)
                    {
                        if (DataModel.AxesMap.ContainsKey(element.actionName))
                        {
                            Debug.Log("key重复，请检查AxesMap文件");
                        }
                        else
                        {
                            DataModel.AxesMap.Add(element.actionName, element.axesName);
                        }
                    }
                }
            }
        }
        
        public static void RefreshInput()
        {

        }
        
        public static void Dispose()
        {
            
        }
        
        /// <summary>
        /// 需要放在Update中执行
        /// </summary>
        public static void CheckInput()
        {
            if (DataModel.InputActions?.Count > 0)
            {
                InputActionEntity inputActionEntity = DataModel.InputActions?.Peek();
                if (inputActionEntity != null)
                {
                    foreach (var element in inputActionEntity.Actions)
                    {
                        InputActionParam inputActionParam = new InputActionParam();
                        float value; 
                        if (DataModel.IsUseMap)
                        {
                            if (!DataModel.AxesMap.ContainsKey(element.Key))
                            {
                                Debug.LogError("action不存在");
                                return;
                            }
                            value = Input.GetAxis(DataModel.AxesMap[element.Key]);
                        }
                        else
                        {
                            value = Input.GetAxis(element.Key);
                        }

                        if (value != 0)
                        {
                            if (element.Value.IsInExecuting)
                            {
                                element.Value.IsInExecuting = true;
                                inputActionParam.AxisValue = value;
                                element.Value.Execute?.Invoke(inputActionParam);
                            }
                            else
                            {
                                element.Value.IsInExecuting = true;
                                inputActionParam.AxisValue = value;
                                element.Value.Start?.Invoke(inputActionParam);
                            }
                            
                            inputActionParam.PreviousFrameAxisValue = value;
                        }
                        else if (element.Value.IsInExecuting)
                        {
                            element.Value.IsInExecuting = false;
                            inputActionParam.AxisValue = value;
                            element.Value.Cancel?.Invoke(inputActionParam);
                            inputActionParam.PreviousFrameAxisValue = value;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 添加一层输入
        /// </summary>
        /// <param name="inputActionEntity">输入的实例</param>
        /// <param name="reserve">是否保留上一层输入</param>
        /// <returns></returns>
        public static bool AddInputAction(InputActionEntity inputActionEntity,bool reserve = false)
        {
            if (inputActionEntity == null)
            {
                return false;
            }

            try
            {
                if (reserve)
                {
                    DataModel.CurrentAction = inputActionEntity;
                    DataModel.NoReserveInputActions.Push(inputActionEntity);
                    var actions = DataModel.InputActions.Peek().Actions;
                    foreach (var element in actions)
                    {
                        if (!inputActionEntity.Actions.ContainsKey(element.Key))
                        {
                            inputActionEntity.Actions.TryAdd(element.Key, element.Value);
                        }
                    }
                }
                else
                {
                    DataModel.CurrentAction = inputActionEntity;
                    DataModel.InputActions.Push(inputActionEntity);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 移除最上层的输入实例
        /// </summary>
        /// <returns></returns>
        public static bool RemoveTopInputAction()
        {
            if (DataModel.InputActions?.Count > 0)
            {
                InputActionEntity inputActionEntity = DataModel.InputActions?.Peek();
                if (inputActionEntity != null)
                {
                    DataModel.InputActions?.Pop();
                    DataModel.NoReserveInputActions?.Pop();
                    DataModel.InputActionsPool.Push(DataModel.CurrentAction);
                }
                else
                {
                    return false;
                }
            }                
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 移除指定的输入实例
        /// </summary>
        /// <param name="inputActionEntity">指定的输入实例</param>
        /// <returns></returns>
        public static bool RemoveInputAction(InputActionEntity inputActionEntity)
        {
            if (inputActionEntity == null || DataModel.InputActions.Contains(inputActionEntity))
            {
                return false;
            }
            
            try
            {
                Stack<InputActionEntity> temp = new Stack<InputActionEntity>();
                Stack<InputActionEntity> noReserveTemp = new Stack<InputActionEntity>();
                //换迭代器或while
                for (int i = 0; i < DataModel.InputActions.Count; i++)
                {
                    var element = DataModel.InputActions.Pop();
                    var noReserveElement = DataModel.NoReserveInputActions.Pop();
                    if (element == inputActionEntity || noReserveElement == inputActionEntity)
                    {
                        break;
                    }
                    temp.Push(element);
                    noReserveTemp.Push(noReserveElement);
                }

                for (int i = 0; i < temp.Count; i++)
                {
                    var element = temp.Pop();
                    DataModel.InputActions.Push(element);
                }
                for (int i = 0; i < noReserveTemp.Count; i++)
                {
                    var element = noReserveTemp.Pop();
                    DataModel.NoReserveInputActions.Push(element);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 慎用！！！获取事件映射的字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAxesMap()
        {
            return DataModel.AxesMap;
        }
        
        /// <summary>
        /// 慎用！！！设置事件映射
        /// </summary>
        /// <param name="axesMap"></param>
        public static void SetAxesMap(Dictionary<string, string> axesMap)
        {
            DataModel.AxesMap = axesMap;
        }

        /// <summary>
        /// 改变事件对应的轴的名字，用于自定义按键
        /// </summary>
        /// <param name="actionName">事件名</param>
        /// <param name="axesName">修改后的轴名字</param>
        /// <returns></returns>
        public static bool ChangeAxesName(string actionName,string axesName)
        {
            if (DataModel.AxesMap.ContainsKey(actionName))
            {
                DataModel.AxesMap[actionName] = axesName;
            }
            else
            {
                Debug.LogError("无法找到action");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 交换两个Action对应的轴
        /// </summary>
        /// <param name="name">Action名字</param>
        /// <param name="anotherName">另一个Action名字</param>
        /// <returns></returns>
        public static bool ExchangeActionName(string name,string anotherName)
        {
            if (DataModel.AxesMap.ContainsKey(name) || DataModel.AxesMap.ContainsKey(anotherName))
            {
                Debug.LogError("name或anotherName不存在");
                return false;
            }

            (DataModel.AxesMap[name], DataModel.AxesMap[anotherName]) = (DataModel.AxesMap[anotherName], DataModel.AxesMap[name]);
            return true;
            //需要回调
        }

        /// <summary>
        /// 主要用于写回硬盘
        /// 将当前事件和轴的对应关系的字典转为List并返回
        /// </summary>
        /// <returns></returns>
        public static List<AxesMapListEntity> GetAxesMapList()
        {
            List<AxesMapListEntity> list = new List<AxesMapListEntity>();
            AxesMapListEntity axesMapListEntity = new AxesMapListEntity();
            foreach (var element in DataModel.AxesMap)
            {
                axesMapListEntity.actionName = element.Key;
                axesMapListEntity.axesName = element.Value;
                list.Add(axesMapListEntity);
            }
            return list;
        }

    }

    /// <summary>
    /// 数据
    /// </summary>
    public  class InputHelperManagerDataModel
    {
        public InputActionEntity CurrentAction;
        public Stack<InputActionEntity> NoReserveInputActions = new Stack<InputActionEntity>();
        /// <summary>
        /// 事件的栈
        /// </summary>
        public Stack<InputActionEntity> InputActions = new Stack<InputActionEntity>();

        /// <summary>
        /// 事件的栈池子
        /// </summary>
        public Stack<InputActionEntity> InputActionsPool = new Stack<InputActionEntity>();
        
        /// <summary>
        /// 是否使用事件映射
        /// 启用时，InputActionEntity的ActionsKey传入的是自定义的AxesMap文件中的Key的值
        /// 关闭时，传入的是InputManager的轴的名字
        /// </summary>
        public bool IsUseMap;

        public Dictionary<string, string> AxesMap = new Dictionary<string, string>();
    }

    /// <summary>
    /// Axes对应的事件组中事件的参数基类
    /// </summary>
    public class InputActionParam
    {
        /// <summary>
        /// Axes轴的值
        /// </summary>
        public float AxisValue;
    
        /// <summary>
        /// 上一帧Axes轴的值
        /// </summary>
        public float PreviousFrameAxisValue;
    }

    /// <summary>
    /// 输入事件类
    /// </summary>
    public class InputActionEntity
    {
        /// <summary>
        /// 输入事件所在界面的名字
        /// </summary>
        public string Name;
        /// <summary>
        /// 输入的事件的字典
        /// key为InputManager中的Axes的Name
        /// Value为事件组
        /// </summary>
        public readonly Dictionary<string, InputActionGroup> Actions = new Dictionary<string, InputActionGroup>();
    }

    
    /// <summary>
    /// 单个Axes对应的事件组
    /// </summary>
    public class InputActionGroup
    {
        /// <summary>
        /// 是否处于执行状态中
        /// </summary>
        public bool IsInExecuting;
        /// <summary>
        /// 执行,轴的不为0时执行
        /// </summary>
        public Action<InputActionParam> Start;
        /// <summary>
        /// 执行,轴的不为0时执行
        /// </summary>
        public Action<InputActionParam> Execute;
        /// <summary>
        /// 取消,轴的值由非0变为0时执行
        /// </summary>
        public Action<InputActionParam> Cancel;
    }

    [Serializable]
    public class AxesMap : ScriptableObject
    {
        [SerializeField]
        public List<AxesMapListEntity> entity;
#if UNITY_EDITOR
        [MenuItem("Assets/Create/InputHelper/AxesMap")]
        static void Create()
        {
            AxesMap axesMap = CreateInstance<AxesMap>();
            axesMap.entity = new List<AxesMapListEntity>();
            ProjectWindowUtil.CreateAsset(axesMap, "AxesMap.asset");
        }
#endif
    }

    [Serializable]
    public struct AxesMapListEntity
    {
        public string actionName;
        public string axesName;
    }
    
}