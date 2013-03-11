using UnityEngine;
using System.Collections;

public enum CameraType { E_CameraNone, E_CameraFixedView, E_CameraThirdPerson, E_CameraFirstPerson };
public enum InputType { E_InputNone, E_InputGamePad, E_InputKeyboard, E_InputTouch };
public enum DifficultyLevel { E_DifficultyEasy, E_DifficultyNormal, E_DifficultyHard };

public class GameSettings : MonoBehaviour
{
	private CameraType m_CameraType = CameraType.E_CameraNone;
	private InputType m_InputType = InputType.E_InputNone;
	private DifficultyLevel m_DifficultyLevel = DifficultyLevel.E_DifficultyNormal;
	
	public CameraType GetCameraType() { return m_CameraType; }
	public void SetCameraType(CameraType _CameraType) { m_CameraType = _CameraType; }
	
	public InputType GetInputType() { return m_InputType; }
	public void SetInputType(InputType _InputType) { m_InputType = _InputType; }
	
	public DifficultyLevel GetDifficultyLevel() { return m_DifficultyLevel; }
	public void SetDifficultyLevel(DifficultyLevel _DifficultyLevel) { m_DifficultyLevel = _DifficultyLevel; }
	
	void Start()
	{
		LoadGameSettings();
		
	}
	
	void LoadGameSettings()
	{
		//Default settings
		//@TODO: serialization from config file
		
		m_CameraType = CameraType.E_CameraThirdPerson;
		m_InputType = InputType.E_InputGamePad;
		m_DifficultyLevel = DifficultyLevel.E_DifficultyNormal;
	}
	
	void Update()
	{
	
	}
}
