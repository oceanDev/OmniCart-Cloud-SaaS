import { Button } from '@/components/Button'

export default function App() {
  return (
    <div className="min-h-screen bg-slate-950 text-white flex flex-col items-center justify-center p-6">
      <div className="p-8 rounded-2xl bg-slate-900 border border-slate-800 shadow-xl max-w-md w-full text-center space-y-6">
        <h1 className="text-3xl font-extrabold tracking-tight text-indigo-400">
          OmniCart Cloud
        </h1>
        <p className="text-slate-400 text-sm">
          React 19 + TypeScript + Tailwind v4 + Enterprise Scaffolding Active!
        </p>
        <div className="flex justify-center gap-4">
          <Button label="Primary Action" variant="primary" onClick={() => alert('Path alias works!')} />
          <Button label="Secondary" variant="secondary" />
        </div>
      </div>
    </div>
  )
}