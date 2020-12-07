import * as React from 'react';

export type ColorType = undefined|'current'|'black'|'white'|'gray'|'red'|'yellow'|'green'|'blue'|'indigo'|'purple'|'pink';

function getIsValueColor(color: ColorType): boolean {
    if (!color) return false;

    if (color == 'black' ||
        color == 'white' ||
        color == 'current') return false;

    return true;
}

type Rounded = "rounded"|"full"|"none";

function getRounded(rounded?: Rounded): string|null {
    if (rounded == null || rounded == "rounded") return "rounded";
    else if (rounded == "full") return "rounded-full"
    else return null;
}
export type NomalButtonProps = {
    children?: React.ReactNode,
    onClick?: (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => void,
    rounded?: Rounded,
    className?: string,
    colorType?: ColorType,
};

export default function NomalButton({children, onClick, rounded, className, colorType}: NomalButtonProps): JSX.Element {

    const isValue = getIsValueColor(colorType);
    const round = getRounded(rounded);

    const inlineStyle: React.CSSProperties = {
        userSelect: "none",
    };

    const styles = [
        "transition duration-500 ease-in-out",
        "text-white",
        "py-2",
        "px-4",
        isValue? `bg-${colorType}-500`: colorType,
        "hover:shadow-md",
    ];
    if (round != null)
        styles.push(round);
    
    if (isValue) 
        styles.push(`hover:bg-${colorType}-400`);
    if (className)
        styles.push(className);

    return (
        <button onClick={onClick} style={inlineStyle} className={styles.join(' ')}>{children}</button>
    );
    
}