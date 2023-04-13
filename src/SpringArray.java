import java.util.Stack;

public class SpringArray {
    public static Spring equivalentSpring(String springExpr) {
        Stack<Spring> stack = new Stack<>();
        Stack<Spring> outStack = new Stack<>();
        Spring current = null;
        for (int i = 0; i < springExpr.length(); i++) {
            char c = springExpr.charAt(i);
            if (c == '{' || c == '[')
                stack.push(new Spring());
            else if (current == null) current = stack.pop();
            else if (c == '}')
                current = current.inSeries(stack.pop());
            else if (c == ']')
                current = current.inParallel(stack.pop());

        }
        return current;
    }

    public static Spring equivalentSpring(String springExpr, Spring[] springs) {
        Stack<Spring> stack = new Stack<>();
        for (int i = 0; i < springExpr.length(); i++) {
            char c = springExpr.charAt(i);
            if (c == '{') {
                stack.push(new Spring());
            } else if (c == '[') {
                stack.push(springs[0]);
            } else if (c == '}') {
                Spring s = stack.pop();
                if (stack.isEmpty()) {
                    return s;
                } else {
                    stack.peek().inSeries(s);
                }
            } else if (c == ']') {
                Spring s = stack.pop();
                if (stack.isEmpty()) {
                    return s;
                } else {
                    stack.peek().inParallel(s);
                }
            }
        }
        return stack.pop();
    }
}
