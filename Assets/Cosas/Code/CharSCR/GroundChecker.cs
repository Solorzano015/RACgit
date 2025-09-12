using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("BoxCast para detectar el suelo")]
    [Tooltip("Medidas del BoxCast (half extents) que se usan para detectar el suelo")]
    public Vector3 boxCastHalfExtents = new Vector3(0.5f, 0.1f, 0.5f);
    [Tooltip("Distancia máxima del BoxCast hacia abajo")]
    public float boxCastDistance = 0.5f;
    [Tooltip("LayerMask que se considera suelo")]
    public LayerMask groundMask;

    // Variable para conocer si el personaje está en el suelo
    public bool IsGrounded { get; private set; }

    void Update()
    {
        // Realiza un BoxCast desde la posición actual (o puedes ajustar el punto de inicio si lo deseas)
        RaycastHit hit;
        // Usamos la rotación del objeto para definir la orientación del box
        // El BoxCast se lanza hacia abajo desde transform.position
        IsGrounded = Physics.BoxCast(transform.position, boxCastHalfExtents, Vector3.down, out hit, transform.rotation, boxCastDistance, groundMask);
    }

    // Dibujar Gizmos para visualizar el BoxCast en la vista de escena
    void OnDrawGizmos()
    {
        // Establece el color del gizmo
        Gizmos.color = Color.cyan;
        // Calcula la posición final del BoxCast
        Vector3 boxOrigin = transform.position;
        Vector3 boxEnd = boxOrigin + Vector3.down * boxCastDistance;

        // Dibujar el box en la posición de origen
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxOrigin, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxCastHalfExtents * 2);

        // Dibujar el box en la posición final
        Matrix4x4 rotationMatrixEnd = Matrix4x4.TRS(boxEnd, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrixEnd;
        Gizmos.DrawWireCube(Vector3.zero, boxCastHalfExtents * 2);

        // Opcional: dibujar una línea entre ambas posiciones
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(boxOrigin, boxEnd);
    }
}
