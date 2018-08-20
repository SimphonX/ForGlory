using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Assets.Scripts.Units;
using System.Text.RegularExpressions;

public class ArcherTest {
    
	[UnityTest]
	public IEnumerator GetUnitGroupFromArcherGroup() {
		var go = new GameObject();
        go.AddComponent<ArcherGroup>();
        LogAssert.Expect(LogType.Exception,new Regex("Transform child"));

        yield return new WaitForFixedUpdate();

        Assert.NotNull(go.GetComponent<UnitGroup>().gameObject);
        

    }
}
