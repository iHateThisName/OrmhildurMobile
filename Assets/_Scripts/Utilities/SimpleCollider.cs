using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SimpleCollider : MonoBehaviour {
    [SerializeField] private List<string> tags;
    //[SerializeField] private LayerMask allowedLayers;

    [SerializeField] private UnityEvent<string> OnTriggerEnterEvent;
    [SerializeField] private UnityEvent<string> OnTriggerExitEvent;

    private void OnTriggerEnter2D(Collider2D collision) {
        string containedTag = tags.FirstOrDefault(t => collision.CompareTag(t));

        if (containedTag != null) {
            OnTriggerEnterEvent?.Invoke(containedTag);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        string containedTag = tags.FirstOrDefault(t => collision.CompareTag(t));

        if (containedTag != null) {
            OnTriggerExitEvent?.Invoke(containedTag);
        }
    }

    private void OnTriggerEnter(Collider other) {
        string containedTag = tags.FirstOrDefault(t => other.CompareTag(t));

        if (containedTag != null) {
            OnTriggerEnterEvent?.Invoke(containedTag);
        }
    }
}
