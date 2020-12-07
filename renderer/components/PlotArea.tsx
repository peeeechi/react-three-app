import React, {useEffect} from 'react';
import plotly from 'plotly.js-dist';

export type PlotAreaProps = {
    plotID: string,
    data: {[key: string]: number[]},
    yRange: number[],
    num: number,
}

export default function PlotArea(props: PlotAreaProps) {

    const layout = {
        xaxis: {
            range: [-1, 1]
        },
        yaxis: {
            range: props.yRange,
        },

    } as any;

    const config = {
        
    } as plotly.Config;

    const init = () => {
        
        if (typeof window !== 'undefined') {
            console.log('init');
            plotly.newPlot(props.plotID,  [], layout, config);
        }
    };

    const test = () => {
        console.log('props.num: ', props.num);
    }

    const drawData = () => {

        console.log('react-before');

        if (typeof window !== 'undefined') {

            console.log('react');
            const plotData = Object.entries(props.data).map(d => {
                return {
                    y: d[1],
                    name: d[0],
                    type: "scattergl",
                } as plotly.ScatterData;
            });

            layout.datarevision ++;

    
            plotly.react(props.plotID, plotData, layout);
        }
    };

    useEffect(() => {
        test();
    }, [props.num]);

    useEffect(() => {
        init();
    }, [props.plotID]);

    useEffect(() => {
        drawData();
    });

    return (
        <>
            <div id={props.plotID}></div>
        </>
    )
}