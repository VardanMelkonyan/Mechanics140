using OpenTK.Mathematics;

namespace mechanics;

internal abstract class Expression
{
    protected Vector3 color;

    public Expression(Vector3 color)
    {
        this.color = color;
    }

    // Default color white
    public Expression()
    {
        color = new Vector3(255, 255, 255);
    }

    public abstract bool PointBelongToExpression(float x, float y);

    public bool PointBelongToExpression(Vector2 point)
    {
        return PointBelongToExpression(point.X, point.Y);
    }

    public Vector3? GetPointColor(int x, int y)
    {
        if (PointBelongToExpression(x, y)) return color;

        return null;
    }
}