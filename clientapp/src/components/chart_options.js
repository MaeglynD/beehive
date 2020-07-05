export default {
	animation: {
		duration: 3,
	},
	hover: {
		animationDuration: 0,
	},
	responsiveAnimationDuration: 0,
	responsive: true,
	elements: {
		point: {
			radius: 0,
			line: {
				tension: 0,
			}
		}
	},
	tooltips: {
		enabled: false
	},
	legend: {
		display: false
	},
	scales: {
		xAxes: [{
			type: 'time',
			distribution: "linear",
			display: false,
		}],
		yAxes: [{
			display: false,
			ticks: {
				min: 0,
				max: 100,
			},
		}],
	},
	maintainAspectRatio: false
};