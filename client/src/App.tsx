import { Card } from '@/components/Card';
import { Button } from '@/components/Button';

export default function App() {
  return (
    <div className="min-h-screen bg-slate-950 text-white flex flex-col items-center justify-center p-6">
      <div className="max-w-md w-full">
        {/* 🔥 Compound Component-এর আসল সৌন্দর্য দেখো: */}
        <Card>
          <Card.Header>
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold uppercase tracking-wider text-indigo-400 bg-indigo-950/60 px-2.5 py-1 rounded-full border border-indigo-800">
                Electronics
              </span>
              <span className="text-xs text-slate-500">In Stock</span>
            </div>
            <Card.Title>Keychron Q1 Pro</Card.Title>
            <Card.Description>
              Wireless Custom Mechanical Keyboard with hot-swappable switches.
            </Card.Description>
          </Card.Header>

          <Card.Content>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-extrabold text-white">$199.00</span>
              <span className="text-sm line-through text-slate-500">$229.00</span>
            </div>
          </Card.Content>

          <Card.Footer>
            <Button as="a" 
              href="https://github.com" 
              target="_blank" 
              label="View Source ↗" 
               variant="secondary" />

               
            <Button label="Add to Cart" variant="primary" onClick={() => alert('Added to cart!')} />
          </Card.Footer>
        </Card>
      </div>
    </div>
  );
}