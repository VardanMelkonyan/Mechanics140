using System.Collections.Generic;
using OpenTK.Mathematics;

namespace mechanics;

internal class ExpressionHandler
{
    private readonly List<Expression> expressions;
    private readonly Surface surface;
    private List<Vector2> inteceptionPoints;

    public ExpressionHandler(Surface surface, List<Vector2> inteceptionPoints)
    {
        this.surface = surface;
        expressions = new List<Expression>();
        this.inteceptionPoints = inteceptionPoints;
    }

    public void Render()
    {
        for (var x = 0; x < surface.width; x++)
        for (var y = 0; y < surface.height; y++)
            foreach (var expression in expressions)
            {
                var color = expression.GetPointColor(x, y);
                if (color.HasValue) surface.SetPixel(x, y, color.GetValueOrDefault());
            }
    }

    public void addExpression(Expression e)
    {
        expressions.Add(e);
    }
}