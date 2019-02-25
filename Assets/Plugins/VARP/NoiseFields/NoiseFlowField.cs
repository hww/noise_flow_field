/* https://github.com/hww/noise_flow_field */

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VARP.NoiseFields
{
    public class NoiseFlowField : MonoBehaviour
    {
        [Header("Field size")]
        public Vector3Int gridSize;
        public float cellSize = 1f;
        public float cellIncrement = 3;
        [Header("Noise")]
        /// <summary>Do not spawn in this radius twice</summary>
        public Vector3 noiseOffset;
        public Vector3 noiseScale = new Vector3(1,1,1);
        [HideInInspector]        
        public Vector3[,,] directions; 
        private FastNoise fastNoise;

        [Header("Particles")]
        public GameObject particlePrefab;
        public int amountOfParticles = 100;
        [HideInInspector]
        public List<FieldParticle> particles;
        public float particleScale = 1;
        /// <summary>Do not spawn in this radius twice</summary>
        public float spawnRadius = 1f;
        public float particleMoveSpeed = 10f;
        public float rotationSpeed = 90f;

        public System.Action onParticlesGenerated;
        
        void Start()
        {
            directions = new Vector3[gridSize.x, gridSize.y, gridSize.z];    
            fastNoise = new FastNoise();
            particles = new List<FieldParticle>(amountOfParticles);
            for (var i = 0; i < amountOfParticles; i++)
            {
                var attempt = 0;
                while (attempt < 100)
                {
                    var randomPos = new Vector3(
                        Random.Range(transform.position.x, transform.position.x + gridSize.x * cellSize),
                        Random.Range(transform.position.y, transform.position.y + gridSize.y * cellSize),
                        Random.Range(transform.position.z, transform.position.z + gridSize.z * cellSize)
                    );

                    if (ValidatePosition(randomPos))
                    {
                        var particle = Instantiate(particlePrefab);
                        particle.transform.position = randomPos;
                        particle.transform.parent = transform;
                        particle.transform.localScale = new Vector3(particleScale,particleScale,particleScale);
                        var ffParticle = particle.GetComponent<FieldParticle>();
                        particles.Add(ffParticle);
                        ffParticle.OnSpawn();
                        break;
                    }
                    else
                    {
                        attempt++;
                    }
                }
            }
            onParticlesGenerated?.Invoke();
        }
        
        private void Update()
        {
            UpdateBoundingVolume();
            CalculateDirections();
            ParticlesBehaviour();
        }

        private Vector3 boundingVolumeMin;
        private Vector3 boundingVolumeMax;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBoundingVolume()
        {
            boundingVolumeMin = transform.position;
            boundingVolumeMax = new Vector3(
                transform.position.x + gridSize.x * cellSize,
                transform.position.y + gridSize.y * cellSize, 
                transform.position.z + gridSize.z * cellSize);
        }
        
        void ParticlesBehaviour()
        {
            var deltaTime = Time.deltaTime;
            for (var i = 0; i < particles.Count; i++)
            {
                var particle = particles[i];
                var partTfrm = particle.transform;
                partTfrm.position = TestEdges(partTfrm.position);
                var particlePos = new Vector3Int(
                    Mathf.FloorToInt(Mathf.Clamp((partTfrm.position.x - transform.position.x) / cellSize, 0f, gridSize.x - 1)),
                    Mathf.FloorToInt(Mathf.Clamp((partTfrm.position.y - transform.position.y) / cellSize, 0f, gridSize.y - 1)),
                    Mathf.FloorToInt(Mathf.Clamp((partTfrm.position.z - transform.position.z) / cellSize, 0f, gridSize.z - 1)));
                particle.ApplyRotation(directions[particlePos.x, particlePos.y, particlePos.z], rotationSpeed, deltaTime);
                particle.moveSpeed = particleMoveSpeed;
                partTfrm.localScale = new Vector3(particleScale, particleScale, particleScale); 
                particle.OnUpdate(deltaTime);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 TestEdges(Vector3 position)
        {
            if (position.x > boundingVolumeMax.x)
                position.x = boundingVolumeMin.x;
            else if (position.x < boundingVolumeMin.x)
                position.x = boundingVolumeMax.x;
            
            if (position.y > boundingVolumeMax.y)
                position.y = boundingVolumeMin.y;
            else if (position.y < boundingVolumeMin.y)
                position.y = boundingVolumeMax.y;
            
            if (position.z > boundingVolumeMax.z)
                position.z = boundingVolumeMin.z;
            else if (position.z < boundingVolumeMin.z)
                position.z = boundingVolumeMax.z;
            return position;
        }
        
        void CalculateDirections()
        {
            if (fastNoise == null) 
                fastNoise = new FastNoise();
            var deltaTime = Time.deltaTime;
            noiseOffset = new Vector3(
                noiseOffset.x + noiseScale.x * deltaTime,
                noiseOffset.y + noiseScale.y * deltaTime,
                noiseOffset.z + noiseScale.z * deltaTime);
            var xOff = 0f;
            for (var x = 0; x < gridSize.x; x++)
            {
                var yOff = 0f;
                for (var y = 0; y < gridSize.y; y++)
                {
                    var zOff = 0f;
                    for (var z = 0; z < gridSize.z; z++)
                    {
                        var noise = fastNoise.GetSimplex(xOff + noiseOffset.x,
                                        yOff + noiseOffset.y, 
                                        zOff + noiseOffset.z) + 1f;
                        var noiseDir = new Vector3(Mathf.Cos(noise * Mathf.PI),
                            Mathf.Sin(noise * Mathf.PI),
                            Mathf.Cos(noise * Mathf.PI)
                        );
                        directions[x,y,z] = Vector3.Normalize(noiseDir);
                        zOff += cellIncrement;
                    }  
                    yOff += cellIncrement;
                }
                xOff += cellIncrement;
            }
        }
        
        // TODO! Remove it or make fast way
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ValidatePosition(Vector3 position)
        {
            float sqrtRadius = spawnRadius * spawnRadius;
            for (var i=0; i< particles.Count; i++)
                if (sqrtRadius > (particles[i].transform.position - position).sqrMagnitude)
                    return false;
            return true;
        }
        
        public bool drawGizmos;
        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
               Gizmos.color = Color.white;
               Gizmos.DrawWireCube(
                   transform.position + 
                   new Vector3(gridSize.x * cellSize * 0.5f,
                       gridSize.y * cellSize * 0.5f,
                       gridSize.z * cellSize * 0.5f),
                   new Vector3(gridSize.x * cellSize, gridSize.y * cellSize, gridSize.z * cellSize));
            }
        }
    }
}