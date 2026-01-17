using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityFunctionSort : MonoBehaviour
{

    private int updateCount = 0;
    private int fixedUpdateCount = 0;


    private void Update()
    {
        updateCount++;
        Debug.Log($"【updateCount】第{updateCount}次调用");
    }

    private void FixedUpdate()
    {
        fixedUpdateCount++;
        Debug.Log($"【fixedUpdateCount】第{fixedUpdateCount}次调用");
    }
}
