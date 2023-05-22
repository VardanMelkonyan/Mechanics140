using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace mechanics;

internal class Utils
{
    public static Random random = new();

    public static Vector2 RandomPoint()
    {
        var res = new Vector2();

        var phi = random.NextDouble() * Math.PI * 2.0;
        var r = random.NextDouble();
        res.X = (float)(r * Math.Cos(phi));
        res.Y = (float)(r * Math.Sin(phi));
        return res;
    }

    // Inital coordinates
    public static Vector2 RandomPoint(Surface surface)
    {
        var res = new Vector2();

        var phi = random.NextDouble() * Math.PI * 2.0;
        var r = random.NextDouble();
        
        var diameter = surface.height * 0.4;
        res.X = (float)(r * Math.Cos(phi) * diameter) + surface.width / 2;
        res.Y = (float)(r * Math.Sin(phi) * diameter) + surface.height / 2;
        return res;
    }

    public static LineSegment RandomLineSegment(Surface surface)
    {
        return new LineSegment(RandomPoint(surface), RandomPoint(surface));
    }

    private static float CalculatePointDistance(float x1, float y1, float x2, float y2)
    {
        // Calculate the distance between two points using the distance formula
        var distanceX = x2 - x1;
        var distanceY = y2 - y1;

        return (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
    }

    private float CalculatePointDistance(Vector2 v1, Vector2 v2)
    {
        return CalculatePointDistance(v1.X, v1.Y, v2.X, v2.Y);
    }

    public static Vector2 CalculateInterceptionPoint(Circle circle, Vector2 origin, Vector2 direction)
    {
        // Calculate the vector from the circle center to the point
        Vector2 fromCenter = origin - new Vector2(circle.centerX, circle.centerY);

        // Calculate the dot product of the direction vector and the vector from the center
        float dotProduct = Vector2.Dot(direction, fromCenter);

        // Calculate the discriminant for the interception point calculation
        float discriminant = dotProduct * dotProduct - direction.LengthSquared * (fromCenter.LengthSquared - circle.radius * circle.radius);

        if (discriminant < 0)
        {
            throw new Exception("Incorrect determinant");
        }
        else
        {
            float t = (-dotProduct - (float)Math.Sqrt(discriminant)) / direction.LengthSquared;
            Vector2 interception = origin + (direction * t);
            return interception;
        }
    }
    
    public static Vector2 CalculateBallTrajectory(float initialHeight, Vector3 initialMomentum, float time)
    {
        float gravity = 10; // Acceleration due to gravity (m/s^2)

        float x = initialMomentum.X * time; // Horizontal position (x)
        float y = (float)(initialHeight + (initialMomentum.Y * time) - (0.5 * gravity * time * time)); // Vertical position (y)

        return new Vector2(x, y);
    }

    public static Vector2 ReflectVector(Vector2 vector, Circle circle, Vector2 point)
    {
        // Calculate the vector from the circle center to the point
        var circleToPointX = point.X - circle.centerX;
        var circleToPointY = point.Y - circle.centerY;

        // Normalize the circle-to-point vector
        var magnitude = (float)Math.Sqrt(circleToPointX * circleToPointX + circleToPointY * circleToPointY);
        var normalizedCircleToPointX = circleToPointX / magnitude;
        var normalizedCircleToPointY = circleToPointY / magnitude;

        // Calculate the dot product of the vector and the normalized circle-to-point vector
        var dotProduct = vector.X * normalizedCircleToPointX + vector.Y * normalizedCircleToPointY;

        // Calculate the reflection vector
        var reflectionVectorX = vector.X - 2 * dotProduct * normalizedCircleToPointX;
        var reflectionVectorY = vector.Y - 2 * dotProduct * normalizedCircleToPointY;

        return new Vector2(reflectionVectorX, reflectionVectorY);
    }


    public class Circle : Expression
    {
        public float centerX;
        public float centerY;

        private readonly float delta;
        public float radius;

        public Circle(float center_x, float center_y, float circle_radius, float delta)
        {
            centerX = center_x;
            centerY = center_y;
            radius = circle_radius;
            this.delta = delta;
        }


        public override bool PointBelongToExpression(float x, float y)
        {
            var distanceSquared = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
            var radiusSquared = radius * radius;

            var diff = distanceSquared - radiusSquared;
            return -delta <= diff && diff <= delta;
        }

        public bool IsInTheCircle(Vector2 point)
        {
            var distanceSquared = (point.X - centerX) * (point.X - centerX) + (point.Y - centerY) * (point.Y - centerY);
            var radiusSquared = radius * radius;

            return distanceSquared <= radiusSquared;
        }
    }
    
    
    public class OuterCircle : Expression
    {
        private Circle _circle;
        public OuterCircle(Circle circle) : base(new Vector3())
        {
            _circle = circle;
        }
        public override bool PointBelongToExpression(float x, float y)
        {
            var distanceSquared = (x - _circle.centerX) * (x - _circle.centerX) + (y - _circle.centerY) * (y - _circle.centerY);
            var radiusSquared = _circle.radius * _circle.radius;

            return distanceSquared >= radiusSquared && !_circle.PointBelongToExpression(x, y);
        }
    }


    // Will not be using for now
    public abstract class Line : Expression
    {
        public float slope;
        public float yIntercept;

        protected Line()
        {
        }

        public Line(float line_slope, float line_yIntercept)
        {
            slope = line_slope;
            yIntercept = line_yIntercept;
        }


        public float CorrespondingY(float x)
        {
            return slope * x + yIntercept;
        }
        
        public float CorrespondingX(float y)
        {
            return (y - yIntercept) / slope;
        }
    }


    public class LineSegment : Line
    {
        private readonly double delta;
        public Vector2 end;

        public Vector2 start;

        public LineSegment(Vector2 start, Vector2 end) : this(start, end, 1)
        {
        }

        public LineSegment(Vector2 start, Vector2 end, double delta)
        {
            this.start = start;
            this.end = end;
            Vector2 vector = end - start;
            slope = vector.Y / vector.X;
            yIntercept = start.Y - slope * start.X;
            this.delta = delta;
        }


        public override bool PointBelongToExpression(float x, float y)
        {
            
            bool isWithinXBoundaries = (x >= start.X ^ x <= end.X);
            bool isWithinYBoundaries = (y >= start.Y ^ y <= end.Y);
            if (isWithinXBoundaries && isWithinYBoundaries)
                return false;

            float expectedY = CorrespondingY(x);
            float diffY = expectedY - y;
            float expectedX = CorrespondingX(y);
            float diffX = expectedX - x;
            return (-delta <= diffY && diffY <= delta) || (-delta <= diffX && diffX <= delta);
        }
    }
    
    public class GravityParabola : Expression
    {
        public Vector2 start;
        public Vector2 momentum0;
        private float delta;
        public GravityParabola(Vector2 start, Vector2 momentum0)
        {
            this.start = start;
            this.momentum0 = momentum0;
        }
        
        // Function to calculate the trajectory equation y(x)
        public float CorrespondingY(float x)
        {
            float g = 10; // Acceleration due to gravity (m/s^2)
            float m = 1; // Mass of the ball

            // Calculate the angle theta of the momentum vector
            double theta = Math.Atan2(momentum0.Y, momentum0.X);

            // Calculate the initial speed
            Vector2 v = Vector2.Divide(momentum0, m);

            // Calculate the vertical displacement y as a function of horizontal position x
            float y = (float)(start.Y + (x * Math.Tan(theta)) - ((g * x * x) / (2 * v.LengthSquared)));

            return y;
        }

        public override bool PointBelongToExpression(float x, float y)
        {
            float expectedY = CorrespondingY(x);
            float diffY = expectedY - y;
            float expectedX = CorrespondingX(y);
            float diffX = expectedX - x;
            return (-delta <= diffY && diffY <= delta) || (-delta <= diffX && diffX <= delta);
        }
        public float CorrespondingX(float y)
        {
            float g = 10; // Acceleration due to gravity (m/s^2)
            float m = 1; // Mass of the ball

            // Calculate the angle theta of the momentum vector
            double theta = Math.Atan2(momentum0.Y, momentum0.X);

            // Calculate the initial speed
            Vector2 v = Vector2.Divide(momentum0, m);

            // Solve the quadratic equation for x
            float a = (float)-0.5 * g / v.LengthSquared;
            float b = (float)Math.Tan(theta);
            float c = y - start.Y;

            float discriminant = b * b - 4 * a * c;

            if (discriminant >= 0)
            {
                float sqrtDiscriminant = (float)Math.Sqrt(discriminant);
                float x1 = (-b + sqrtDiscriminant) / (2 * a);
                float x2 = (-b - sqrtDiscriminant) / (2 * a);

                // Choose the positive solution for x
                float x = Math.Max(x1, x2);
                return x;
            }

            // No real solutions for x
            return float.NaN;
        }
    }
}