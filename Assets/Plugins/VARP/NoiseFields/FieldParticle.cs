/* https://github.com/hww/noise_flow_field */

using System.Runtime.CompilerServices;
using UnityEngine;

namespace VARP.NoiseFields
{
    public class FieldParticle : MonoBehaviour
    {
        public float moveSpeed;
        public int audioBand;
        public MeshRenderer meshRenderer;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnSpawn()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate(float deltaTime)
        {
            var pos = transform.forward * moveSpeed * deltaTime;
            if (float.IsNaN(pos.x))
                Debug.LogError($"{transform.position} {transform.forward} {moveSpeed} {deltaTime}");
            transform.position += transform.forward * moveSpeed * deltaTime;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyRotation(Vector3 rotation, float rotationSpeed, float deltaTime)
        {
            var rot = Quaternion.LookRotation(rotation);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotationSpeed * deltaTime);
        }
        
    }
}