using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Configuración de la bala
    [SerializeField]
    private GameObject explosion; // Prefab del efecto de explosión
    [SerializeField]
    private float speed = 600.0f; // Velocidad de la bala
    [SerializeField]
    private float lifeTime = 3.0f; // Tiempo de vida antes de destruirse
    public int damage = 50; // Daño infligido por la bala (para referencia futura si se necesita)

    void Start()
    {
        // Destruir la bala automáticamente después de un tiempo
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Mover la bala hacia adelante según su dirección y velocidad
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Obtener el punto de contacto de la colisión
        ContactPoint contact = collision.contacts[0];

        // Instanciar el efecto de explosión en el punto de contacto
        if (explosion != null)
        {
            Instantiate(explosion, contact.point, Quaternion.identity);
        }

        // Destruir la bala tras la colisión
        Destroy(gameObject);
    }
}
