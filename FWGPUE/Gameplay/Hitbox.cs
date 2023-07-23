using System.Collections;

namespace FWGPUE.Gameplay;

/// <summary>
/// Used to test intersection.
/// <para> Currently just a set of circles. </para>
/// </summary>
class Hitbox : IEnumerable<Circle>
{
    public List<Circle> Circles = new();

    public void Add(Circle circle)
    {
        Circles.Add(circle);
    }

    public bool Intersects(Hitbox other)
    {
        // test circle intersection between all circles in this and other
        foreach (var circle in Circles)
        {
            foreach (var otherCircle in other.Circles)
            {
                if ((circle.position - otherCircle.position).Length() <= circle.radius + otherCircle.radius)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #region ienumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Circles).GetEnumerator();
    }
    public IEnumerator<Circle> GetEnumerator()
    {
        return ((IEnumerable<Circle>)Circles).GetEnumerator();
    }
    #endregion ienumerable
}
