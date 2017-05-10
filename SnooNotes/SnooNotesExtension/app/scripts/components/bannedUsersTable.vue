<template>
    <div class="sn-search-table" v-if="adminSubs && adminSubs.length > 0">
        <button @click="toggleAll" class="sn-toggle-all">Check/Uncheck all</button>
        <label v-for="sub in adminSubs" class="sn-search-sub-label">
            <input type="checkbox" :value="sub.SubName" v-model="searchSubs" />
            {{sub.SubName}}
        </label>
        <paginator :rows="rowsPerPage" :page="currentPage" :total="totalRows"></paginator>
        <div class="sn-table-area">
            
            <table>
                <thead class="md-table-header">
                    <tr>
                        <th class="sn-sortable" :class="{'sn-sorted-desc':sort=='date' && !ascending, 'sn-sorted-asc':sort=='date' && ascending}"><i class="material-icons sn-sort-icon">arrow_downward</i>Date</th>
                        <th class="sn-sortable" :class="{'sn-sorted-desc':sort=='username' && !ascending, 'sn-sorted-asc':sort=='username' && ascending}">User</th>
                        <th class="sn-sortable" :class="{'sn-sorted-desc':sort=='subname' && !ascending, 'sn-sorted-asc':sort=='subname' && ascending}">Subreddit</th>
                        <th class="sn-sortable" :class="{'sn-sorted-desc':sort=='bannedby' && !ascending, 'sn-sorted-asc':sort=='bannedby' && ascending}">Banned By</th>
                        <th class="sn-sortable" :class="{'sn-sorted-desc':sort=='reason' && !ascending, 'sn-sorted-asc':sort=='reason' && ascending}">Reason</th>
                        <th>Banned For</th>
                        <th>Additional Info</th>
                    </tr>
                </thead>
                <tbody v-if="!loadingResults">
                    <tr v-for="ban in searchResults">
                        <td>{{ban.BanDate}}</td>
                        <td>{{ban.UserName}}</td>
                        <td>{{ban.SubName}}</td>
                        <td>{{ban.BannedBy}}</td>
                        <td>{{ban.BanReason}}</td>
                        <td>{{ban.ThingURL}}</td>
                        <td>{{ban.AdditionalInfo}}</td>
                    </tr>
                </tbody>
            </table>
            <sn-loading v-if="loadingResults"></sn-loading>
        </div>
    </div>
</template>
<script>
import Paginator from './paginator.vue';
import LoadingSpinner from './loadingSpinner.vue';
import axios from 'axios';

export default {
  components:{'paginator':Paginator, 'sn-loading': LoadingSpinner},
  data(){
      return{
          rowsPerPage: 25,
          currentPage : 1,
          totalRows: 0,
          searchSubs: [],
          loadingResults: false,
          searchResults: [],
          sort: 'date',
          ascending: false,
          subreddits: this.$select('snoonotes_info.modded_subs as subreddits')
      }
  },
  computed:{
      adminSubs: function(){
        return this.subreddits.filter((s)=>{return s.IsAdmin})
      }
  },
  methods:{
      toggleAll(){
          if(this.searchSubs.length == this.adminSubs.length){
              this.searchSubs = [];
          }
          else{
              this.searchSubs = this.adminSubs.map((s)=>{return s.SubName});
          }
      },
      searchBannedUsers(){
          this.loadingResults = true;
          let queryString = "";
          if(this.searchSubs.length != this.adminSubs.length){
              queryString += "subreddits=" + this.searchSubs.join() + "&";
          }
          queryString += "limit="+this.rowsPerPage +"&page="+this.currentPage;
          //TODO other params
          axios.get('BotBan/Search/User?' + queryString).then((response)=>{
              this.loadingResults = false;
              this.searchResults = []; //just in case errors
              let results = response.data;
              this.currentPage = results.CurrentPage;
              this.rowsPerPage = results.ResultsPerPage;
              this.totalRows = results.TotalResults;
              this.searchResults = results.DataTable;
          },(err)=>{this.searchResults = []; this.loadingResults = false;})
      }
  },
  created: function(){
      this.toggleAll();
      this.searchBannedUsers();
  }
}
</script>
