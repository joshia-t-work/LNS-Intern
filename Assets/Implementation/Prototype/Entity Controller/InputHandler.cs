using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LNS.Entities
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] UnityEvent<Vector2> _WASDInput;
        [SerializeField] UnityEvent<Vector2> _mouseAimInput;
        [SerializeField] UnityEvent _mouseClickInput;
        [SerializeField] UnityEvent _spacebarInput;
        private void Update()
        {
            _WASDInput.Invoke(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
            _mouseAimInput.Invoke(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
            if (Input.GetMouseButtonDown(0))
            {
                _mouseClickInput.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _spacebarInput.Invoke();
            }
        }
    }
}
