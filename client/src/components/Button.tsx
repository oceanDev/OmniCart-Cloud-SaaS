interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  label: string;
  variant?: 'primary' | 'secondary';
}

export const Button = ({ label, variant = 'primary', className = '', ...props }: ButtonProps) => {
  const baseStyles = 'px-5 py-2.5 rounded-lg font-medium transition-all duration-200 shadow-sm active:scale-95 cursor-pointer';
  const variants = {
    primary: 'bg-indigo-600 hover:bg-indigo-700 text-white',
    secondary: 'bg-slate-800 hover:bg-slate-700 text-slate-100 border border-slate-700',
  };

  return (
    <button className={`${baseStyles} ${variants[variant]} ${className}`} {...props}>
      {label}
    </button>
  );
};