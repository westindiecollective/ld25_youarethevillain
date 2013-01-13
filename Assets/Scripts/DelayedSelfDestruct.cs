using UnityEngine;
using System.Collections;

public class DelayedSelfDestruct : MonoBehaviour
{
	public float m_DelayInSeconds = 0.0f;
	private bool m_TriggeredWait = false;

	void Start()
	{

	}

	void Update()
	{
		if (!m_TriggeredWait)
		{
			StartCoroutine( DelayedDestroy( m_DelayInSeconds ) );

			m_TriggeredWait = true;
		}
	}

	IEnumerator DelayedDestroy( float _DelayInSeconds )
	{
		yield return new WaitForSeconds(_DelayInSeconds);

		Destroy(gameObject);
	}
}
