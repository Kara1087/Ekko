using UnityEngine;

namespace _Ekko.Scripts.Gameplay.Systems
{
    public struct LandData
    {
        public float force { private set; get;}
        public float minForce { private set; get;}
        public float maxForce { private set; get;}
        public Transform landObject { private set; get;}
        
        public LandingType landingType { private set; get;}
        
        public LandData(float force, float minForce, float maxForce, Transform landObject, LandingType landingType)
        {
            this.force = force;
            this.minForce = minForce;
            this.maxForce = maxForce;
            this.landObject = landObject;
            this.landingType = landingType;
        }
    }
}