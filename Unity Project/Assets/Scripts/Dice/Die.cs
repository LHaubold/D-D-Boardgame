using UnityEngine;
using System.Collections;

/// <summary>
/// Die base class to determine if a die is rolling and to calculate it's current value
/// </summary>
public class Die : MonoBehaviour
{
	// current value, 0 is undetermined (die is rolling) or invalid.
	public int value = 0;	
	
	// normalized (hit)vector from die center to upper side in local space is used to determine what side of a specific die is up/down = value
    protected Vector3 localHitNormalized;
	// hitVector check margin
    protected float validMargin = 0.45F;

	// true is die is still rolling
    public bool rolling
    {
        get
        {
            return !(GetComponent<Rigidbody>().velocity.sqrMagnitude < .1F && GetComponent<Rigidbody>().angularVelocity.sqrMagnitude < .1F);
        }
    }

	// calculate the normalized hit vector and should always return true
    protected bool localHit
    {
        get
        {
			// create a Ray from straight above this Die , moving downwards
            Ray ray = new Ray(transform.position + (new Vector3(0, 2, 0) * transform.localScale.magnitude), Vector3.up * -1);
            RaycastHit hit = new RaycastHit();
			// cast the ray and validate it against this die's collider
            if (GetComponent<Collider>().Raycast(ray, out hit, 3 * transform.localScale.magnitude))
            {
				// we got a hit so we determine the local normalized vector from the die center to the face that was hit.
				// because we are using local space, each die side will have its own local hit vector coordinates that will always be the same.
                localHitNormalized = transform.InverseTransformPoint(hit.point.x, hit.point.y, hit.point.z).normalized;
                return true;
            }
			// in theory this should never happen
            return false;
        }
    }

	// calculate this die's value
    void GetValue()
    {
		// value = 0 -> undetermined or invalid
        value = 0;
        float delta = 1;
		// start with side 1 going up.
        int side = 1;
        Vector3 testHitVector;
		// check all sides of this die, the side that has a valid hitVector and smallest x,y,z delta (if more sides are valid) will be the closest and this die's value
        do
        {
			// get testHitVector from current side
            testHitVector = HitVector(side);
            if (testHitVector != Vector3.zero)
            {
				// this side has a hitVector so validate the x,y and z value against the local normalized hitVector using the margin.
                if (valid(localHitNormalized.x, testHitVector.x) &&
                    valid(localHitNormalized.y, testHitVector.y) &&
                    valid(localHitNormalized.z, testHitVector.z))
                {
					// this side is valid within the margin, check the x,y, and z delta to see if we can set this side as this die's value
					// if more than one side is within the margin we have to use the closest as the right side
                    float nDelta = Mathf.Abs(localHitNormalized.x - testHitVector.x) + Mathf.Abs(localHitNormalized.y - testHitVector.y) + Mathf.Abs(localHitNormalized.z - testHitVector.z);
                    if (nDelta < delta)
                    {
                        value = side;
                        delta = nDelta;
                    }
                }
            }
			// increment side
            side++;
			// if we got a Vector.zero as the testHitVector we have checked all sides of this die
        } while (testHitVector != Vector3.zero);
    }

    void Update()
    {
		// determine the value if the die is not rolling
        if (!rolling && localHit)
            GetValue();
    }


	// validate a test value against a value within a specific margin.
    protected bool valid(float t, float v)
    {
        if (t > (v - validMargin) && t < (v + validMargin))
            return true;
        else
            return false;
    }

	// method to get a die side hitVector.
    protected Vector3 HitVector(int side)
    {
        switch (side)
        {
            case 1: return new Vector3(0F, 0F, 1F);
            case 2: return new Vector3(0F, -1F, 0F);
            case 3: return new Vector3(-1F, 0F, 0F);
            case 4: return new Vector3(1F, 0F, 0F);
            case 5: return new Vector3(0F, 1F, 0F);
            case 6: return new Vector3(0F, 0F, -1F);
        }
        return Vector3.zero;
    }
	
}
