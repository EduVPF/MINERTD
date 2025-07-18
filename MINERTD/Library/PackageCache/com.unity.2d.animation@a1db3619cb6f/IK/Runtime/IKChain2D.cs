using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace UnityEngine.U2D.IK
{
    /// <summary>
    /// Class for storing data for a 2D IK Chain.
    /// </summary>
    [MovedFrom("UnityEngine.Experimental.U2D.IK")]
    [Serializable]
    public class IKChain2D
    {
        [SerializeField]
        [FormerlySerializedAs("m_Target")]
        Transform m_EffectorTransform;

        [SerializeField]
        [FormerlySerializedAs("m_Effector")]
        Transform m_TargetTransform;

        [SerializeField]
        int m_TransformCount;

        [SerializeField]
        Transform[] m_Transforms;

        [SerializeField]
        Quaternion[] m_DefaultLocalRotations;

        [SerializeField]
        Quaternion[] m_StoredLocalRotations;

        /// <summary>
        /// Array of lengths in the IK Chain.
        /// </summary>
        protected float[] m_Lengths;

        /// <summary>
        /// Get and set the transform used as the IK Effector.
        /// </summary>
        public Transform effector
        {
            get => m_EffectorTransform;
            set => m_EffectorTransform = value;
        }

        /// <summary>
        /// Get and set the transform used as the IK Target.
        /// </summary>
        public Transform target
        {
            get => m_TargetTransform;
            set => m_TargetTransform = value;
        }

        /// <summary>
        /// Get the transforms that are used in the IK Chain.
        /// </summary>
        public Transform[] transforms => m_Transforms;

        /// <summary>
        /// Get the root transform for the IK Chain.
        /// </summary>
        public Transform rootTransform
        {
            get
            {
                if (m_Transforms != null && transformCount > 0 && m_Transforms.Length == transformCount)
                    return m_Transforms[0];
                return null;
            }
        }

        Transform lastTransform
        {
            get
            {
                if (m_Transforms != null && transformCount > 0 && m_Transforms.Length == transformCount)
                    return m_Transforms[transformCount - 1];
                return null;
            }
        }

        /// <summary>
        /// Get and Set the number of transforms in the IK Chain.
        /// </summary>
        public int transformCount
        {
            get => m_TransformCount;
            set => m_TransformCount = Mathf.Max(0, value);
        }

        /// <summary>
        /// Returns true if the IK Chain is valid. False otherwise.
        /// </summary>
        public bool isValid => Validate();

        /// <summary>
        /// Gets the length of the IK Chain.
        /// </summary>
        public float[] lengths
        {
            get
            {
                if (isValid)
                {
                    PrepareLengths();
                    return m_Lengths;
                }

                return null;
            }
        }

        bool Validate()
        {
            if (effector == null)
                return false;
            if (transformCount == 0)
                return false;
            if (m_Transforms == null || m_Transforms.Length != transformCount)
                return false;
            if (m_DefaultLocalRotations == null || m_DefaultLocalRotations.Length != transformCount)
                return false;
            if (m_StoredLocalRotations == null || m_StoredLocalRotations.Length != transformCount)
                return false;
            if (rootTransform == null)
                return false;
            if (lastTransform != effector)
                return false;
            return !target || !IKUtility.IsDescendentOf(target, rootTransform);
        }

        /// <summary>
        /// Initialize the IK Chain.
        /// </summary>
        public void Initialize()
        {
            if (effector == null || transformCount == 0 || IKUtility.GetAncestorCount(effector) < transformCount - 1)
                return;

            m_Transforms = new Transform[transformCount];
            m_DefaultLocalRotations = new Quaternion[transformCount];
            m_StoredLocalRotations = new Quaternion[transformCount];

            Transform currentTransform = effector;
            int index = transformCount - 1;

            while (currentTransform && index >= 0)
            {
                m_Transforms[index] = currentTransform;
                m_DefaultLocalRotations[index] = currentTransform.localRotation;

                currentTransform = currentTransform.parent;
                --index;
            }
        }

        void PrepareLengths()
        {
            Transform currentTransform = effector;
            int index = transformCount - 1;

            if (m_Lengths == null || m_Lengths.Length != transformCount - 1)
                m_Lengths = new float[transformCount - 1];

            while (currentTransform && index >= 0)
            {
                Transform parent = currentTransform.parent;
                if (parent && index > 0)
                    m_Lengths[index - 1] = parent.TransformVector((Vector2)currentTransform.localPosition).magnitude;

                currentTransform = currentTransform.parent;
                --index;
            }
        }

        /// <summary>
        /// Restores the IK Chain to it's default pose.
        /// </summary>
        /// <param name="targetRotationIsConstrained">True to constrain the target rotation. False otherwise.</param>
        public void RestoreDefaultPose(bool targetRotationIsConstrained)
        {
            int count = targetRotationIsConstrained ? transformCount : transformCount - 1;
            for (int i = 0; i < count; ++i)
                m_Transforms[i].localRotation = m_DefaultLocalRotations[i];
        }

        /// <summary>
        /// Explicitly stores the local rotation.
        /// </summary>
        public void StoreLocalRotations()
        {
            for (int i = 0; i < m_Transforms.Length; ++i)
                m_StoredLocalRotations[i] = m_Transforms[i].localRotation;
        }

        /// <summary>
        /// Blend between Forward Kinematics and Inverse Kinematics.
        /// </summary>
        /// <param name="finalWeight">Weight for blend</param>
        /// <param name="targetRotationIsConstrained">True to constrain target rotation. False otherwise.</param>
        public void BlendFkToIk(float finalWeight, bool targetRotationIsConstrained)
        {
            int count = targetRotationIsConstrained ? transformCount : transformCount - 1;
            for (int i = 0; i < count; ++i)
                m_Transforms[i].localRotation = Quaternion.Slerp(m_StoredLocalRotations[i], m_Transforms[i].localRotation, finalWeight);
        }
    }
}
