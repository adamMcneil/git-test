using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private int xInput = 0;
    private int lastXInput = 0;
    private int zInput = 0;
    private int lastZInput = 0;
    public float speed = 20.0f;
    public float jumpStrength = 5.0f;
    private bool isOnGround = true;
    float yRotation = 0;
    float xRotation = 0;

    private Rigidbody body;
    [SerializeField] GameObject focalPoint;
    [SerializeField] GameObject block;

    [SerializeField] Material material;

    float sensitivity = 0.75f;

    private UIManager UIManagerScript;
    static private int numberOfPlayers = 0;

    private int myCubesMade = 0;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        body = GetComponent<Rigidbody>();
        UIManagerScript = GameObject.Find("UI Manager").GetComponent<UIManager>();
        UIManagerScript.UpdatePlayerCountUI(++numberOfPlayers);

        if (isLocalPlayer)
        { 
            focalPoint.SetActive(true);
            material.color = new Color(Random.Range(0, 100)/100f, Random.Range(0, 100)/100f, Random.Range(0, 100)/100f);
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            Move();
            Jump();
            RotateCamera();
            CheckMakeBlock();
        }
    }

    void RotateCamera()
    {
        float xMouse = Input.GetAxis("Mouse X") * sensitivity;
        float yMouse = Input.GetAxis("Mouse Y") * sensitivity;

        yRotation += xMouse;
        xRotation -= yMouse;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        //focalPoint.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        focalPoint.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    void CheckMakeBlock()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isServer)
            {
                GameObject spawnBlock = Instantiate(block, transform.position + focalPoint.transform.forward, block.transform.rotation);
                NetworkServer.Spawn(spawnBlock);
                CorrectBlockColor(spawnBlock);
            }
            else
            {
                SpawnBlockCmd(transform.position + focalPoint.transform.forward);
            }
        }
    }

    void Move()
    {
        xInput = 0;
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            xInput = -lastXInput;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            xInput = -1;
            lastXInput = xInput;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            xInput = 1;
            lastXInput = xInput;
        }

        zInput = 0;
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.W))
        {
            zInput = -lastZInput;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            zInput = -1;
            lastZInput = zInput;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            zInput = 1;
            lastZInput = zInput;
        }
        Vector3 moveVector = (focalPoint.transform.forward * zInput * Time.deltaTime + focalPoint.transform.right * xInput * Time.deltaTime) * speed;
        moveVector.y = body.velocity.y;
        body.velocity = moveVector;
       
    }

    void Jump()
    {

        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            body.velocity = new Vector3(body.velocity.x, jumpStrength, body.velocity.z);
            isOnGround = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }

    [Command]
    void SpawnBlockCmd(Vector3 position)
    { 
        GameObject spawnBlock = Instantiate(block, position, block.transform.rotation);
        //spawnBlock.GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
        NetworkServer.Spawn(spawnBlock);
    }

    [ClientRpc]
    void CorrectBlockColor(GameObject block)
    {
    }
}
