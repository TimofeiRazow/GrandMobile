using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Codebase.Controllers
{
    public class CameraService
    {
        private CinemachineCamera _camera;

        public void BindCamera(CinemachineCamera camera)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        }

        public void BindToTarget(Transform target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (_camera == null)
                throw new Exception("Перед установкой цели, нужно установить камеру!");

            _camera.Follow = target;
        }

        public void Unbind() =>
            _camera = null;
    }
}