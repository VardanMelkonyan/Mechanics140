import org.junit.Test;

import static org.junit.Assert.assertArrayEquals;
import static org.junit.Assert.assertEquals;
public class UnitTests {
    @Test
    public void testDefaultConstructor() {
        Spring spring = new Spring();
        assertEquals(1.0, spring.getK(), 0.00001);
    }

    @Test
    public void testOverloadedConstructor() {
        Spring spring = new Spring(2.5);
        assertEquals(2.5, spring.getK(), 0.00001);
    }

    @Test
    public void testGetterAndSetter() {
        Spring spring = new Spring();
        spring.setK(3.5);
        assertEquals(3.5, spring.getK(), 0.00001);
    }

    @Test
    public void testMoveWithUnitMass() {
        Spring spring = new Spring();
        double[] expected = {0.0, 0.5, 0.866025, 1.0, 0.866025, 0.5, 1.22465e-16, -0.5, -0.866025, -1.0};
        double[] actual = spring.move(2*Math.PI, 0.1, 1.0);
        assertArrayEquals(expected, actual, 0.00001);
    }

    @Test
    public void testMoveWithSpecifiedMass() {
        Spring spring = new Spring();
        double[] expected = {0.0, 0.5, 0.866025, 1.0, 0.866025, 0.5, 1.22465e-16, -0.5, -0.866025, -1.0};
        double[] actual = spring.move(0, 2*Math.PI, 0.1, 1.0, 0.0, 1.0);
        assertArrayEquals(expected, actual, 0.00001);
    }

    @Test
    public void testInSeries() {
        Spring spring1 = new Spring(2.0);
        Spring spring2 = new Spring(3.0);
        Spring equivalent = spring1.inSeries(spring2);
        assertEquals(5.0, equivalent.getK(), 0.00001);
    }

    @Test
    public void testInParallel() {
        Spring spring1 = new Spring(2.0);
        Spring spring2 = new Spring(3.0);
        Spring equivalent = spring1.inParallel(spring2);
        assertEquals(1.2, equivalent.getK(), 0.00001);
    }

    @Test
    public void testEquivalentSpringWithDefaultSprings() {
        String springExpr = "{{}[]}";
        Spring equivalent = SpringArray.equivalentSpring(springExpr);
        assertEquals(1.0, equivalent.getK(), 0.00001);
    }

    @Test
    public void testEquivalentSpringWithSpecifiedSprings() {
        Spring[] springs = {new Spring(2.0), new Spring(3.0)};
        String springExpr = "{[{}]}";
        Spring equivalent = SpringArray.equivalentSpring(springExpr, springs);
        assertEquals(5.0, equivalent.getK(), 0.00001);
    }
}
