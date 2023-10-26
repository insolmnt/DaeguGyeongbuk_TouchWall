using UnityEngine;

public class FreeCamera : MonoBehaviour {
	public string DataKey = "CameraPosition";
	public float lookSpeed = 2f;
	public float moveSpeed = 5f;
	public float sprintSpeed = 20f;

	float	m_yaw;
	float	m_pitch;

	[System.Serializable]
	public class FreeCameraData
    {
		public Vector3 Position;
		public Vector3 Rotation;

		public float Fov = 60;
    }

	public FreeCameraData Data;
	private Vector3 DefaultPosition;
	private Vector3 DefaultRotation;

	public Camera ForwardCamera;
	public Camera TopCamera;

	static public bool inputCaptured;

	void CaptureInput() {
		inputCaptured = true;

		m_yaw = transform.eulerAngles.y;
		m_pitch = transform.eulerAngles.x;

		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowMessage("카메라 이동 활성화\n[Home]키 - 기본값\n[End]키 - 저장", 5f);
		}
	}

	void ReleaseInput() {
		inputCaptured = false;
		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowMessage("카메라 이동 종료");
		}
	}

	void Start()
	{
		DefaultPosition = transform.position;
		DefaultRotation = transform.localEulerAngles;

		Load();
    }

    public void Save()
	{
		Data.Position = transform.position;
		Data.Rotation = transform.localEulerAngles;

		DataManager.SetData<FreeCameraData>(DataKey, Data);

		if(UIManager.Instance != null)
        {
			UIManager.Instance.ShowMessage("저장 완료");
        }
    }
    public void Load()
    {
		Data = DataManager.GetData<FreeCameraData>(DataKey);
		if(Data == null)
        {
			Data = new FreeCameraData();
			Data.Position = DefaultPosition;
			Data.Rotation = DefaultRotation;
		}


		transform.position = Data.Position;
		transform.localEulerAngles = Data.Rotation;

		m_yaw = transform.eulerAngles.y;
		m_pitch = transform.eulerAngles.x;
	}

	public void SetFov()
    {
		ForwardCamera.fieldOfView = Data.Fov;
		TopCamera.fieldOfView = Data.Fov;
		TopCamera.transform.localEulerAngles = new Vector3(-Data.Fov, 0, 0);
    }
	public void Home()
    {
		Data.Position = DefaultPosition;
		Data.Rotation = DefaultRotation;

		transform.position = Data.Position;
		transform.localEulerAngles = Data.Rotation;

		Data.Fov = 60;
		SetFov();

		m_yaw = transform.eulerAngles.y;
		m_pitch = transform.eulerAngles.x;
		if (UIManager.Instance != null)
		{
			UIManager.Instance.ShowMessage("기본값 복원 (End키를 눌러서 저장 필요)");
		}
	}

	void Update() {
		if(SensorManager.Instance.IsShowDebugMode == false/* && SensorManager.Instance.IsShowGroupDebugMode == false*/)
        {
			return;
        }

        if (inputCaptured)
		{
			if (Input.GetKeyDown(KeyCode.Home))
			{
				Home();
			}
			if (Input.GetKeyDown(KeyCode.End))
			{
				Save();
			}
		}

        if (Input.GetKeyDown(KeyCode.F9) && Input.GetKey(KeyCode.LeftShift) == false)
        {
			inputCaptured = !inputCaptured;
            if (inputCaptured)
            {
				CaptureInput();

			}
            else
            {
				ReleaseInput();

			}

		}

		if (!inputCaptured)
			return;

        if (Input.GetMouseButton(1))
		{
			var rotStrafe = Input.GetAxis("Mouse X");
			var rotFwd = Input.GetAxis("Mouse Y");

			m_yaw = (m_yaw + lookSpeed * rotStrafe) % 360f;
			m_pitch = (m_pitch - lookSpeed * rotFwd) % 360f;
			transform.rotation = Quaternion.AngleAxis(m_yaw, Vector3.up) * Quaternion.AngleAxis(m_pitch, Vector3.right);

			var speed = Time.deltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
			var forward = speed * Input.GetAxis("Vertical");
			var right = speed * Input.GetAxis("Horizontal");
			var up = speed * ((Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f));
			transform.position += transform.forward * forward + transform.right * right + Vector3.up * up;


			float scroll = Input.GetAxis("Mouse ScrollWheel");
			if(scroll != 0)
            {
				Data.Fov -= scroll * Time.deltaTime * 50f;
				SetFov();
            }
		}
	}
}
