namespace Veldrid.RenderDemo
{
    public class TestItem
    {
        public string Name { get; set; }
        public AbstractThing Thing { get; set; }
        public AbstractThing Thing2 { get; set; }

    }

    public abstract class AbstractThing
    {
        public int ID { get; set; }
    }

    public abstract class AbstractThing<T> : AbstractThing
    {

        public T Value { get; set; }
    }

    public class FloatThing : AbstractThing<float> { }
    public class StringThing : AbstractThing<string> { }
    public class IntThing : AbstractThing<int> { }
}
