using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

    // 캐릭터 속도
    Vector3 velocity;

    // 캐릭터에게 Rigidbody 속성을 구현
    Rigidbody myRigidbody;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    // 속도 지정.
    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    // 인자로 받은 포인트 캐릭터의 시야 높이로 조정하여 보도록 하기.
    public void LookAt(Vector3 lookPoint)
    {
        // 캐릭터가 위나 아래가 아닌 정면을 바라보도록 바라보는 포인트를 조정
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }


    void FixedUpdate()
    {
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);
    }
}
