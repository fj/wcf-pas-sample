using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace WsdlOverrideUrlPoc
{
    class WsdlExtensionsConfig : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get
            {
                return typeof(WsdlExtensions);
            }
        }

        protected override object CreateBehavior()
        {
            return new WsdlExtensions(this); ;
        }

        [ConfigurationProperty("location", DefaultValue = null)]
        public Uri Location
        {
            get
            {
                return (Uri)base["location"];
            }
            set
            {
                base["location"] = value;
            }
        }

        [ConfigurationProperty("singleFile", DefaultValue = false)]
        public bool SingleFile
        {
            get
            {
                return (bool)base["singleFile"];
            }
            set
            {
                base["singleFile"] = value;
            }
        }
    }
}
