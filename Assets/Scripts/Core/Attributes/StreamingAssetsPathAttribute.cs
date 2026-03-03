using System;
using UnityEngine;

namespace LedenevTV
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StreamingAssetsPathAttribute : PropertyAttribute
    {
        public string[] AllowedExtensions { get; }

        public StreamingAssetsPathAttribute(params string[] allowedExtensions)
        {
            AllowedExtensions = allowedExtensions;
        }
    }
}