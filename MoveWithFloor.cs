using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithFloor : MonoBehaviour
{

    public CharacterController player;

    Vector3 groundPosition;
    Vector3 lastGroundPosition;
    Quaternion actualRot;
    Quaternion lastRot;
    public int PlatformLayerMask;
    string groundName;
    string lastGrondName;

    public Vector3 originOffset;
    public float factorDivision = 4.2f; 


    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<CharacterController>();   
    }

    // Update is called once per frame
    void Update()
    {
        
        if (player.isGrounded)
        {
            RaycastHit hit;

            if (Physics.SphereCast(transform.position + originOffset, player.radius / factorDivision, -transform.up, out hit))
            {
                GameObject groundedIn = hit.collider.gameObject;
                groundName = groundedIn.name;
                groundPosition = groundedIn.transform.position;
                actualRot = groundedIn.transform.rotation;

                if (hit.collider.gameObject.layer == PlatformLayerMask)
                {
                    if (groundPosition != lastGroundPosition && groundName == lastGrondName)
                    {
                        this.transform.position += groundPosition - lastGroundPosition;

                        //Con esto solucionamos el bug que causa el CharacterController, cuando esta activo no permite actualizar la posicion del personaje
                        player.enabled = false;
                        player.transform.position = this.transform.position;
                        player.enabled = true;
                    }

                    if (actualRot != lastRot && groundName == lastGrondName)
                    {
                        Debug.Log("Probando");
                        var newRot = this.transform.rotation * (actualRot.eulerAngles - lastRot.eulerAngles);
                        this.transform.RotateAround(groundedIn.transform.position, Vector3.up, newRot.y);

                        //Con esto solucionamos el bug que causa el CharacterController, cuando esta activo no permite actualizar la posicion del personaje
                        player.enabled = false;
                        player.transform.position = this.transform.position;
                        player.enabled = true;
                    }
                }

                lastGrondName = groundName;
                lastGroundPosition = groundPosition;
                lastRot = actualRot;

            }

        }
        else if (!player.isGrounded)
        {
            lastGrondName = null;
            lastGroundPosition = Vector3.zero;
            lastRot = Quaternion.Euler(0, 0, 0);
        }

    }

    private void OnDrawGizmos()
    {
        CharacterController player = GetComponent<CharacterController>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + originOffset, player.radius / factorDivision);
    }
}
