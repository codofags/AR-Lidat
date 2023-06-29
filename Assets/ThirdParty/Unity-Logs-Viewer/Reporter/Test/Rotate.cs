using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{
	Vector3 angle;
    public float speed = 1f;

	void Start()
	{
		angle = transform.eulerAngles;
	}

	void Update()
	{
		angle.y += Time.deltaTime * 100 * speed;
		transform.eulerAngles = angle;
	}

}
