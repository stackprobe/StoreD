package tests;

public class Test0001 {
	public static void main(String[] args) {
		try {
			test01();
		} catch (Throwable e) {
			e.printStackTrace();
		}
	}

	private static void test01() {
		int c = 10;
		//Runnable a = () -> System.out.println(c); // error
		c++;
		//a.run();
	}
}
