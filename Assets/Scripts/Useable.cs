using UnityEngine;
using System.Collections;

public abstract class Useable : MonoBehaviour
{
	public enum UseableType { E_UseableNone, E_UseableSwing, E_UseableThrow, E_UseableFire, E_UseablePlace };
	
	public abstract UseableType GetUseableType();
	
	public abstract void Use();
}
