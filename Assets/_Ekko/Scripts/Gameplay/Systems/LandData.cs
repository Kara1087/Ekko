using UnityEngine;

namespace _Ekko.Scripts.Gameplay.Systems
{
    public struct LandData
    {
        public float force { private set; get;}
        public Transform landObject { private set; get;}
        
        public LandingType landingType { private set; get;}
        
        public LandData(float force, Transform landObject, LandingType landingType)
        {
            this.force = force;
            this.landObject = landObject;
            this.landingType = landingType;
        }
    }
}