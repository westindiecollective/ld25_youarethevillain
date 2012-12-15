using UnityEngine;
using System.Collections;

public class NextLevelTimer : MonoBehaviour {
	
	public int m_WaitTimeInSeconds = 0;
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
				StartCoroutine( instance.LoadNextLevelAsync( m_WaitTimeInSeconds ) );
			}
			
			m_TriggeredWait = true;
		}
	}
}
