public class Spring {
    private double k = 1.0;

    public Spring() {
    }

    public Spring(double k) {
        this.k = k;
    }

    public double getK() {
        return k;
    }

    public void setK(double k) {
        this.k = k;
    }

    public double[] move(double t, double dt, double x0, double v0) {
        return move(0, t, dt, x0, v0);
    }

    public double[] move(double t, double dt, double x0) {
        return move(t, dt, x0, 0);
    }

    public double[] move(double t0, double t1, double dt, double x0, double v0) {
        return move(t0, t1, dt, x0, v0, 1);
    }

    public double[] move(double t0, double t1, double dt, double x0, double v0, double m) {
        double omega = Math.sqrt(k / m);
        int length = (int) ((t1 - t0) / dt) + 1;
        double[] coordinates = new double[length];
        for (int i = 0; i < length; i++) {
            double time = t0 + i * dt;
            // Wikipedia check needed
            coordinates[i] = x0 * Math.cos(omega * (time - t0)) + v0 / omega * Math.sin(omega * (time * t0));
        }
        return coordinates;
    }

    public Spring inParallel(Spring that) {
        double stiffness = this.getK() + that.getK();
        return new Spring(stiffness);
    }

    public Spring inSeries(Spring that) {
        double stiffness = (this.getK() * that.getK()) / (this.getK() + that.getK());
        return new Spring(stiffness);
    }
}
