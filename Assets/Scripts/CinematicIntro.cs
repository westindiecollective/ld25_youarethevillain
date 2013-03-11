using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CinematicIntro : MonoBehaviour
{
	public GameObject m_GameManagerPrefab = null;
	private LevelManager m_LevelManager = null;
	
	private bool m_IsActiveSequence = false;
	
	[System.Serializable]
	public class CinematicSlide
	{
		public Texture2D m_SlideTexture;
		public float m_SlideDurationInSeconds;
	}
	
	public List<CinematicSlide> m_IntroSlideShow;
	private int m_CurrentSlideIndex = -1;
	private float m_CurrentSlideTimer = 0.0f;
	private Texture2D m_CurrentSlideTexture = null;
	
	void Start()
	{
		StartSequence();
	}
	
	void OnLevelWasLoaded(int _level)
	{
		//InitGameSequence();
	}
	
	void StartSequence()
	{
		LevelManager levelManager = (LevelManager)FindObjectOfType( typeof(LevelManager) );
		if (levelManager == null && m_GameManagerPrefab != null)
		{
			GameObject gameManager = Instantiate(m_GameManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			levelManager = gameManager.GetComponent<LevelManager>();
		}
		
		m_LevelManager = levelManager;
		
		m_IsActiveSequence = (m_LevelManager != null);
		
		InitCamera();
		StartSlide(0);
	}
	
	void EndSequence()
	{
		m_IsActiveSequence = false;
			
		m_LevelManager.LoadNextLevel();
	}
	
	void InitCamera()
	{
		//GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
		Camera mainCamera = Camera.main;
		mainCamera.orthographic = true;
	}
	
	void StartSlide(int _SlideIndex)
	{
		CinematicSlide introSlide = m_IntroSlideShow[_SlideIndex];
		m_CurrentSlideIndex = _SlideIndex;
		m_CurrentSlideTimer = introSlide.m_SlideDurationInSeconds;
		m_CurrentSlideTexture = introSlide.m_SlideTexture;
	}
	
	void EndSlide()
	{
		int slideCount = m_IntroSlideShow.Count;
		int nextSlideIndex = m_CurrentSlideIndex+1;
		
		if (nextSlideIndex < slideCount)
		{
			StartSlide(nextSlideIndex);
		}
		else
		{
			EndSequence();
		}
	}
	
	void UpdateSlideShow(float _DeltaTime)
	{
		m_CurrentSlideTimer -= _DeltaTime;
		
		if (m_CurrentSlideTimer <= 0.0f)
		{
			m_CurrentSlideTimer = 0.0f;
			EndSlide();
		}
	}
	
	void Update()
	{
		float deltaTime = Time.deltaTime;
		
		if (m_IsActiveSequence)
		{
			//@TODO: check input to skip slides
			
			UpdateSlideShow(deltaTime);
		}
	}
	
	void OnGUI()
	{
		if (m_IsActiveSequence)
		{
			GUI.DrawTexture(new Rect(0.0f, 0.0f, Screen.width, Screen.height), m_CurrentSlideTexture);
		}
	}
}
