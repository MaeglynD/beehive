<template>
	<v-app>
		<v-navigation-drawer
			:mini-variant=true
			:clipped=true
			v-model="drawer"
			left
			floating
			app
			mobile-break-point="0"
			color="#3b322c"
		>
			<v-list three-line>
				<v-tooltip 
					right
					transition="slide-x-reverse-transition"
					v-for="(x, i) in navbar"
					:key="i"
					color="#3b322c"
					class="b-tooltip"
				>
					<template v-slot:activator="{ on }">
						<v-list-item 
							slot="activator" 
							v-on="on" 
							height="88" 
							@click="x.method" 
							v-ripple
						>
							<v-icon color="#9e938b">
								mdi-{{x.icon}}
							</v-icon>
						</v-list-item>
					</template>
					<span>
						{{ x.text }}
					</span>
				</v-tooltip>
			</v-list>
		</v-navigation-drawer>
		<div class="b-container">
			<div 
				class="b-tile-container elevation-10"
				v-for="(x, i) in hive_data"
				:key="`tile-${i}`"
				:style="{ backgroundColor: x.Dead ? '#da4167' : '#48e5c2' }"
			>
				<div class="b-chart-container">
					<span class="b-type">
						{{ x.Type }}
					</span>
					<line-chart 
						:chart-data="x.chart_data"
						:options="options"
						class="b-chart"
						v-if="show_graphs"
					></line-chart>
					<div class="b-tile-info">
						<img :src="require(`./assets/${x.Type}.png`)">
						<span class="b-health">
							{{ Math.round(x.Health * 100) / 100 }}
						</span>
						<span class="b-percentage" v-if="x.chart_data.datasets[0].data.length > 1">
							-{{ percentage(x.chart_data.datasets[0].data) }}%
						</span>
					</div>
					<v-btn
						@click="(sheet_info.index = i), (sheet = true)"
						class="b-attack"
						text
						height="30"
						:disabled="x.Dead"
					>
						attack
					</v-btn>
				</div>
			</div>
			<v-bottom-sheet v-model="sheet" inset max-width="600">
				<v-sheet 
					class="b-dialog"
					height="100px"
					color="#3b322c"
				>
					<v-slider
						v-model="sheet_info.slider"
						thumb-color="#e28413"
						color="#e28413"
						class="b-slider"
						thumb-label="always"
					>
						<template v-slot:thumb-label="{ value }">
							{{ value }}%
						</template>
					</v-slider>
					<div 
						class="b-bee-info"
						v-if="hive_data"
					>
						<span class="b-type">
							{{ hive_data[sheet_info.index].Type }} bee #{{ sheet_info.index }}
						</span>
						Currently {{ Math.round(hive_data[sheet_info.index].Health * 100) / 100 }} health
					</div>
					<v-btn
						color="#e28413"
						text
						@click="damage_bee(sheet_info.index, Math.round(Math.random() * 100))"
					>
						damage random
					</v-btn>
					<v-btn
						color="#e28413"
						text
						class="b-btn"
						@click="damage_bee(sheet_info.index, sheet_info.slider)"
					>
						damage {{ sheet_info.slider }}
					</v-btn>
				</v-sheet>
			</v-bottom-sheet>
			<v-snackbar
				v-model="snackbar"
				color="red"
			>
				{{ snackbar_text }}
			</v-snackbar>
		</div>
	</v-app>
</template>

<script>

import axios from 'axios';
import options from './components/chart_options.js'
import chart_template from './components/chart_template.js'
import LineChart from './components/line_chart.vue';

export default {

	components: {
		LineChart,
	},

	data() {
		return {
			chart_template,
			options,
			sheet: false,
			show_graphs: false,
			is_fullscreen: false,
			init: false,
			snackbar: false,
			drawer: true,
			hive_data: null,
			snackbar_text: '',
			sheet_info: {
				bee: 'Attack drone #4',
				index: 0,
				slider: 50,
			},
			navbar: [
				{ icon: 'play', text: 'damage all bees', method: this.get_damage_all },
				{ icon: 'refresh', text: 'refresh the hive', method: this.get_reset },
				{ icon: 'fullscreen', text: 'fullscreen', method: this.toggle_fullscreen },
			],
		}
	},

	created() {
		// Initialise
		axios.get('/Beehive/GetBeehive')
			.then(({data}) => {
				let storage = localStorage.getItem('hive_data');
				this.update_data(data);
				
				// Check if storage matches actual data, load it if so
				if (data.reduce((a, b) => a + b.Health, 0) !== 3000 && storage) {
					storage = JSON.parse(storage);
					this.hive_data.forEach((x, i) => x.chart_data.datasets[0].data = storage[i])
				}
			})
			.catch((err) => this.handle_err(err));
	},

	methods: {
		handle_err(err) {
			// Show error in snackbar
			this.snackbar = true;
			this.snackbar_text = err; 
		},
		damage_bee(Index, Damage) {
			// Damages an individual bee with a set amount chosen from the frontend
			axios.post('/Beehive/PostDamageOne', { Index, Damage })
				.then(({ data }) => this.update_data(data))
				.catch((err) => this.handle_err(err));

			this.sheet = false;
		},
		percentage(data) {
			// Get the '-x%'
			return 100 - Math.round((data[data.length - 1].y / (data[data.length - 2].y / 100)) * 100) / 100;
		},
		update_data(data) {
			// Update hive_data 
			this.hive_data = data.map((x, i) => {

				// If it has already been initialised, it will already contain
				// a chart template, so we adjust the template when updating
				if (this.init) {
					x.chart_data = this.hive_data[i].chart_data;
					const history = x.chart_data.datasets[0].data;

					// If it isn't dead and has a changed value
					if (!this.hive_data[i].Dead && x.Health !== history[history.length - 1].y) {
						// Actual data the chart samples from
						x.chart_data.datasets[0].data = [
							...history,
							{x: history[history.length - 1].x + 1, y: x.Health }
						];
					}
					return x;
				}

				// Add chart templates to data
				if (!this.init) {
					x.chart_data = JSON.parse(JSON.stringify(this.chart_template));
					x.chart_data.datasets[0].data = [{ x: 0, y: x.Health }];
				}
				return x;
			});

			// Save history of data in local storage, so graphs can sample them on reload
			localStorage.setItem(
				'hive_data',
				JSON.stringify(this.hive_data.map(x => x.chart_data.datasets[0].data))
			);
			this.init = true;

			// Update the graph in the markup 
			this.show_graphs = false;
			this.$nextTick(() => this.show_graphs = true);
		},
		async get_reset() {
			// Reset hive_data
			await axios.get('/Beehive/GetReset')
				.then(({ data }) => {
					this.init = false;
					this.update_data(data);
				})
				.catch((err) => this.handle_err(err))
		},
		async get_damage_all() {
			// Damage all bee's randomly
			await axios.get('/Beehive/GetDamageAll')
				.then(({ data }) => this.update_data(data))
				.catch((err) => this.handle_err(err))
		},
		async toggle_fullscreen() {
			// Toggles fullscreen
			const elem = document.documentElement;
			if (!this.is_fullscreen) {
				if (elem.requestFullscreen) {
					elem.requestFullscreen();
				} else if (elem.mozRequestFullScreen) {
					elem.mozRequestFullScreen();
				} else if (elem.webkitRequestFullscreen) {
					elem.webkitRequestFullscreen();
				} else if (elem.msRequestFullscreen) {
					elem.msRequestFullscreen();
				}
			} else {
				if (document.exitFullscreen) {
					document.exitFullscreen();
				} else if (document.mozCancelFullScreen) {
					document.mozCancelFullScreen();
				} else if (document.webkitExitFullscreen) {
					document.webkitExitFullscreen();
				} else if (document.msExitFullscreen) {
					document.msExitFullscreen();
				}
			}
			this.is_fullscreen = !this.is_fullscreen;
		},
	}
};
</script>

<style lang="scss">
	@import './assets/global.scss';
</style>
