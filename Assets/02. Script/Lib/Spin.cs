using UnityEngine;
using System.Collections;


public class Spin : MonoBehaviour {
    public SpinRotationEnum SpinRotation;
    public float Speed = 10.0f;

    Coroutine S = null;
    void OnEnable()
    {
        if(Speed != 0)
        {
            Play(Speed);
        }
    }

    IEnumerator SpinCoroutine()
    {
        while (true)
        {
            yield return null;
            switch (SpinRotation)
            {
                case SpinRotationEnum.X_RIGHT:
                    transform.localEulerAngles += Vector3.right * Speed * Time.deltaTime;
                    break;
                case SpinRotationEnum.Y_UP:
                    transform.localEulerAngles += Vector3.up * Speed * Time.deltaTime;
                    break;
                case SpinRotationEnum.Z_FORWARD:
                    //transform.Rotate(Vector3.forward * Speed * Time.deltaTime);
                    transform.localEulerAngles += Vector3.forward * Speed * Time.deltaTime;
                    break;
            }
        }
    }

    public void Play(float speed)
    {
        this.Speed = speed;
        if(S != null)
        {
            StopCoroutine(S);
        }
        if(speed != 0)
        {
            S = StartCoroutine(SpinCoroutine());
        }
    }

    public void Stop()
    {
        if (S != null)
        {
            StopCoroutine(S);
        }
    }

    public enum SpinRotationEnum
    {
        X_RIGHT, Y_UP, Z_FORWARD
    }
}
