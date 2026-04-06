using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pattern
{
    public class CameraFollow : Singleton<CameraFollow>
    {
        public Transform target;
        public float lerpSpeed = 5.0f;
        [SerializeField] private bool enable25DView = false;
        [SerializeField, Range(0f, 80f)] private float tiltAngleX = 35f;
        [SerializeField, Range(-45f, 45f)] private float yawAngleY = 0f;
        [SerializeField] private float followHeight = 6f;
        [SerializeField] private float followDistance = 8f;
        [SerializeField] private Vector2 framingOffset = new Vector2(0f, -1f);

        private Vector3 offset;
        private Vector3 targetPos;
        private Vector3 shakeOffset = Vector3.zero;

        private float shakeTimer = 0f;
        private float shakeMagnitude = 0f;

        private Camera cam;
        private float defaultOrthographicSize;
        private Coroutine zoomCoroutine;
        private Transform lastTarget;
        private bool isOffsetInitialized;


        [Button("Test Camera Shake")]
        private void TestShake()
        {
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.Shake(0.2f, 1f);
                Debug.Log("Camera Shake triggered via Odin!");
            }
        }

        [Button("Test Zoom Out")]
        private void TestZoomOut()
        {
            Zoom(defaultOrthographicSize * 1.2f, 0.5f);
            Debug.Log("Zoom Out triggered");
        }

        [Button("Test Shake + Zoom Combo")]
        private void TestShakeZoomCombo()
        {
            Shake(0.25f, 0.3f);
            Zoom(defaultOrthographicSize * 0.9f, 0.5f);
            Debug.Log("Shake + Zoom combo triggered");
        }

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
            RebuildFollowOffset();
            ApplyViewRotationImmediate();
            lastTarget = target;
            isOffsetInitialized = true;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (!isOffsetInitialized || lastTarget != target)
            {
                RebuildFollowOffset();
                lastTarget = target;
                isOffsetInitialized = true;
            }

            if (enable25DView)
            {
                ApplyViewRotationImmediate();
                Vector3 frame = new Vector3(framingOffset.x, framingOffset.y, 0f);
                targetPos = target.position + frame + offset;
            }
            else
            {
                targetPos = target.position + offset;
            }

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

        private void RebuildFollowOffset()
        {
            if (target == null)
                return;

            if (!enable25DView)
            {
                offset = transform.position - target.position;
                return;
            }

            offset = new Vector3(0f, followHeight, -Mathf.Abs(followDistance));
        }

        private void ApplyViewRotationImmediate()
        {
            if (!enable25DView)
                return;

            transform.rotation = Quaternion.Euler(tiltAngleX, yawAngleY, 0f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            RebuildFollowOffset();
            ApplyViewRotationImmediate();
        }
#endif

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
