

// Own Custom props

type ButtonOwnProps<E extends React.ElementType = 'button'> = {
    as?: E;
    label: string;
    variant?: 'primary' | 'secondary' | 'danger';
    className?: string;
};

// Polymorphic Props: Own + Tags Own props

export type ButtonProps<E extends React.ElementType = 'button'> = ButtonOwnProps<E> &
  Omit<React.ComponentProps<E>, keyof ButtonOwnProps<E>>;


export const Button = <E extends React.ElementType = 'button'> ({as, label, variant = 'primary', className = '', ...props}: ButtonProps<E>) => {

    const Component = as || 'button';

    const baseStyles = 'inline-flex items-center justify-center px-5 py-2.5 rounded-lg font-medium transition-all duration-200 shadow-sm active:scale-95 cursor-pointer no-underline text-center';
    const variants = {
      primary: 'bg-indigo-600 hover:bg-indigo-700 text-white',
      secondary: 'bg-slate-800 hover:bg-slate-700 text-slate-100 border border-slate-700',
      danger: 'bg-rose-600 hover:bg-rose-700 text-white shadow-rose-900/20',
    };

    return (
        <Component className={`${baseStyles} ${variants[variant]} ${className}`} {...props}>
            {label}
        </Component>
    );

}
