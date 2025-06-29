using System.Collections;
using UnityEngine;

namespace Pattern
{
    public class CameraFollow : Singleton<CameraFollow>
    {
        public Transform target;
        public float lerpSpeed = 5.0f;

        private Vector3 offset;
        private Vector3 targetPos;
        private Vector3 shakeOffset = Vector3.zero;

        private float shakeTimer = 0f;
        private float shakeMagnitude = 0f;

        private Camera cam;
        private float defaultOrthographicSize;
        private Coroutine zoomCoroutine;

        protected override void Awake()
        {
            base.Awake(); 

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            cam = Camera.main;
            if (cam != null)
                defaultOrthographicSize = cam.orthographicSize;
        }

        private void Start()
        {
            if (target == null) return;
            offset = transform.position - target.position;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            targetPos = target.position + offset;

            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                shakeOffset = Random.insideUnitSphere * shakeMagnitude;
                shakeOffset.z = 0f;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }

            transform.position = Vector3.Lerp(transform.position, targetPos + shakeOffset, lerpSpeed * Time.deltaTime);
        }

        // 📸 Shake
        public void Shake(float duration, float magnitude)
        {
            shakeTimer = duration;
            shakeMagnitude = magnitude;
        }

        // 🔍 Zoom in/out
        public void Zoom(float targetSize, float duration)
        {
            if (cam == null) return;

            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(ZoomRoutine(targetSize, duration));
        }

        private IEnumerator ZoomRoutine(float targetSize, float duration)
        {
            float startSize = cam.orthographicSize;
            float time = 0f;

            while (time < duration)
            {
                cam.orthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            cam.orthographicSize = targetSize;

            yield return new WaitForSeconds(0.5f);
            
            time = 0f;
            while (time < duration)
            {
                cam.orthographicSize = Mathf.Lerp(targetSize, defaultOrthographicSize, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            cam.orthographicSize = defaultOrthographicSize;
        }
    }
}
