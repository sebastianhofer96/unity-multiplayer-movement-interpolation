using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private float m_movementSpeed = 1.0f;
    [SerializeField] private float m_movementThreshold = 0.1f;
    [SerializeField] private Vector2 m_sizeBounds = new Vector2(1f, 30f);
    [SerializeField] private Vector2 m_horizontalBounds = new Vector2(-5.0f, 5.0f);
    [SerializeField] private Vector2 m_verticalBounds = new Vector2(-5.0f, 5.0f);

    private Vector3 m_initialPosition;
    private float m_initialSize;

    private void Start()
    {
        m_initialPosition = m_camera.transform.position;
        m_initialSize = m_camera.orthographicSize;
    }

    private void Update()
    {
        // left mouse button pressed for translation
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseX) < m_movementThreshold) mouseX = 0f;
            if (Mathf.Abs(mouseY) < m_movementThreshold) mouseY = 0f;

            float newPositionX = m_camera.transform.position.x - mouseX * m_movementSpeed;
            float newPositionZ = m_camera.transform.position.z - mouseY * m_movementSpeed;

            newPositionX = Mathf.Max(m_horizontalBounds.x, Mathf.Min(newPositionX, m_horizontalBounds.y));
            newPositionZ = Mathf.Max(m_verticalBounds.x, Mathf.Min(newPositionZ, m_verticalBounds.y));

            m_camera.transform.position = new Vector3(newPositionX, m_camera.transform.position.y, newPositionZ);
        }

        // mouse scrollwheel for zoom
        m_camera.orthographicSize -= Input.mouseScrollDelta.y;
        m_camera.orthographicSize = Mathf.Max(m_sizeBounds.x, Mathf.Min(m_camera.orthographicSize, m_sizeBounds.y));
    }

    public void Reset()
    {
        m_camera.transform.position = m_initialPosition;
        m_camera.orthographicSize = m_initialSize;
    }
}