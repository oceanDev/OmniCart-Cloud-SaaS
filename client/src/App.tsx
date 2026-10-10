import { Card } from '@/components/Card';
import { Button } from '@/components/Button';
import { useProducts } from './features/products/useProducts';

export default function App() {

  // Data 

  const {data: products, isLoading, isError, error } = useProducts();
  console.log('Backend response from .NET 9:', products);

  return (
    <div className="min-h-screen bg-slate-950 text-white p-8">

      {/* Header Section */}

      <div className="max-w-6xl mx-auto mb-10 text-center space-y-2">
        <h1 className="text-4xl font-extrabold text-indigo-400 tracking-tight">
          OmniCart Live Storefront
        </h1>
        <p className="text-slate-400 text-sm">
          Connected directly to .NET 9 Web API using TanStack Query v5
        </p>
      </div>

      <div className="max-w-6xl mx-auto">

        {/* Loading state: Spinner */}

        { isLoading && (
          <div className="flex justify-center items-center py-20">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-500"></div>
            <span className="ml-4 text-slate-300 font-medium">Loading products from .NET 9...</span>
          </div>
        )}

        {/* Error State */}

        {isError && (
          <div className="p-6 rounded-2xl bg-rose-950/40 border border-rose-800 text-rose-300 text-center max-w-lg mx-auto">
            <h3 className="font-bold text-lg mb-1">Failed to connect to backend!</h3>
            <p className="text-sm text-rose-400/80 mb-4">
              {error instanceof Error ? error.message : 'Unknown network error'}
            </p>
            <p className="text-xs text-slate-400">
              Plese ensure your Api is running and CORS is Enabled !!
            </p>
          </div>
        )}

        {/* Success State */}

        {products && (

           <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">

            { products.map((product) => 

                <Card key={product.id}> 
                  
                  <Card.Header>

                    <div className="flex items-center justify-between">

                       <span className="text-xs font-semibold uppercase tracking-wider text-indigo-400 bg-indigo-950/60 px-2.5 py-1 rounded-full border border-indigo-800"> 
                          {product.category || 'General'}
                       </span>

                        <span className="text-xs text-emerald-400 font-medium">
                            Stock: { product.stockQuantity}
                        </span>
                    </div>

                    <Card.Title> { product.name} </Card.Title>
                    <Card.Description> { product.description } </Card.Description>

                  </Card.Header>

                  {/* Content */}

                   <Card.Content>
                      <div className="flex items-baseline gap-2">
                        <span className="text-3xl font-extrabold text-white">
                          ${ product.price.toFixed(2)}
                        </span>
                      </div>
                    </Card.Content>

                  {/* Card Footer */}

                  <Card.Footer>

                    <Button 
                      as="a"
                      href={`/products/${ product.id }`} 
                      label="View Details" 
                      variant="secondary" 
                    />

                    <Button 
                      label="Add to Cart" 
                      variant="primary" 
                      onClick={() => alert(`Added ${product.name} to cart!`)} 
                    />

                  </Card.Footer>

                </Card>

            )}

           </div>

        )}


      </div>


    </div>
  );
}