using InputHelper;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Example : MonoBehaviour
{
    public bool isUseMap;

    private void Awake()
    {
        AxesMap axesMap = AssetDatabase.LoadAssetAtPath<AxesMap>("Assets/InputHelperExample/AxesMap.asset");

        InputHelperManager.Initialization(isUseMap, axesMap);
        Application.targetFrameRate = 60;
    }
    
    private void Start()
    {
        InputActionEntity inputActionEntity = new InputActionEntity();
        InputActionGroup inputActionGroup = new InputActionGroup();
        inputActionEntity.Name = "Main";
        inputActionGroup.Start = (inputActionParam) =>
        {
            Debug.Log("Start");
        };
        inputActionGroup.Execute = (inputActionParam) =>
        {
            Debug.Log("Execute");
        };
        inputActionGroup.Cancel = (inputActionParam) =>
        {
            Debug.Log("Cancel");
        };

        inputActionEntity.Actions.Add("Test", inputActionGroup);

        InputHelperManager.AddInputAction(inputActionEntity);
    }
    
    void Update()
    {
        InputHelperManager.CheckInput();
    }
}