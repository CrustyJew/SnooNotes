<template>
    <div class="sn-search-table" v-if="adminSubs && adminSubs.length > 0">
        <button @click="toggleAll" class="sn-toggle-all">Check/Uncheck all</button>
        <label v-for="sub in adminSubs" class="sn-search-sub-label">
            <input type="checkbox" :value="sub.SubName" v-model="searchSubs" />
            {{sub.SubName}}
        </label>
        <paginator :rows="rowsPerPage" :page="currentPage" :total="totalRows"></paginator>
        <div class="sn-table-area">
            
            <md-table md-sort="date" md-sort-type="desc">
                <md-table-header>
                    <md-table-row>
                        <md-table-head md-sort-by="date">Date</md-table-head>
                        <md-table-head md-sort-by="username">User</md-table-head>
                        <md-table-head md-sort-by="subname">Subreddit</md-table-head>
                        <md-table-head md-sort-by="bannedby">Banned By</md-table-head>
                        <md-table-head md-sort-by="reason">Reason</md-table-head>
                        <md-table-head>Banned For</md-table-head>
                        <md-table-head>Additional Info</md-table-head>
                    </md-table-row>
                </md-table-header>
                
                <md-table-body v-if="!loadingResults">
                    <md-table-row v-for="(ban, banIndex) in searchResults" :md-item="ban" :key="banIndex">
                        <md-table-cell>{{ban.BanDate}}</md-table-cell>
                        <md-table-cell>{{ban.UserName}}</md-table-cell>
                        <md-table-cell>{{ban.SubName}}</md-table-cell>
                        <md-table-cell>{{ban.BannedBy}}</md-table-cell>
                        <md-table-cell>{{ban.BanReason}}</md-table-cell>
                        <md-table-cell>{{ban.ThingURL}}</md-table-cell>
                        <md-table-cell>{{ban.AdditionalInfo}}</md-table-cell>
                    </md-table-row>
                </md-table-body>
            </md-table>
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
