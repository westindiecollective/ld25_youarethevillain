using UnityEngine;
using System.Collections;

public class LevelTimer : MonoBehaviour
{
	public float m_WaitTimeInSeconds = 0.0f;
	public int m_LevelToLoadIndex = 1;
	private bool m_TriggeredWait = false;

	void Start()
	{
	}

	void Update()
	{
		if (!m_TriggeredWait)
		{
			LevelManager instance = (LevelManager)FindObjectOfType (typeof (LevelManager));
			if (instance)
			{
				StartCoroutine( instance.LoadLevelAsync( m_LevelToLoadIndex, m_WaitTimeInSeconds ) );
			}

			m_TriggeredWait = true;
		}
	}
}
