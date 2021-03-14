import React from "react";
import plotly from "plotly.js";
import Plot, { Figure } from "react-plotly.js";

export type RealtimeChartPlotlyProps = {
  plotId: string;
  length: number;
};
export type RealtimeChartPlotlyState = {
  layout: plotly.Layout;
  data: plotly.PlotData[];
  frames: plotly.Frame[];
  config: plotly.Config;
  figure: Figure;
  plotCount: number;
};

const maxPlotNum = 50;

// const Plot = createPlotlyComponent(plotly);

export default class RealtimeChartPlotly extends React.Component<RealtimeChartPlotlyProps, RealtimeChartPlotlyState> {
  constructor(props: RealtimeChartPlotlyProps) {
    super(props);

    this.state = {
      layout: {} as plotly.Layout,
      data: [
          {
            //   x: [1],
              y: [1],
              type: "scatter",
              mode: "lines",
              name: "test"
          } as plotly.ScatterData
      ],
      config: {
        staticPlot: true,
      } as plotly.Config,
      frames: [],
      figure: {} as Figure,
      plotCount: 0,
    };
  }

  private update = () => {};

  static getDerivedStateFromProps(props: RealtimeChartPlotlyProps, state: RealtimeChartPlotlyState) {
      console.log("getDerivedStateFromProps");
      console.dir(props);
      console.dir(state);
  }

  componentDidMount() {
    // Simulate realtime data
    setInterval(() => {
      let newRealtimeData = [...this.state.data];

    //   (newRealtimeData[0].x as number[]).push(this.state.plotCount);
    //   if(newRealtimeData[0].x.length > maxPlotNum) (newRealtimeData[0].x as number[]).shift();
      (newRealtimeData[0].y as number[]).push(Math.floor(Math.random() * 6) + 1);
      if(newRealtimeData[0].y.length > maxPlotNum) (newRealtimeData[0].y as number[]).shift();
      const newLayout = Object.assign({}, this.state.layout) as any;
      newLayout.datarevision++;
      this.setState({ data: newRealtimeData, layout: newLayout, plotCount: this.state.plotCount + 1 });
    }, 1);
  }

  render() {
    return (
      <Plot
        divId={this.props.plotId}
        // style={{ width: "100%", height: "100%" }}
        useResizeHandler={true}
        data={this.state.data}
        config={this.state.config}
        layout={this.state.layout}
        frames={this.state.frames}
        onInitialized={(figure) => this.setState({figure: figure})}
        onUpdate={(figure) => this.setState({figure: figure})}
      />
    );
  }
}
